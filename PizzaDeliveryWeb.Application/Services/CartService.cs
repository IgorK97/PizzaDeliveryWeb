using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class CartService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CartService> _logger;
        public CartService(IUnitOfWork uow, ILogger<CartService> logger)
        {
            _uow = uow;
            _logger = logger;
        }
        public async Task<CartDto> GetOrCreateCartAsync(string clientId)
        {

            var order = await _uow.Orders.GetCartAsync(clientId);
            if (order == null)
            {
                //return MapToCartDto(order);
                order = new Order { 
                    ClientId = clientId,
                    Id=0,
                    Price=0,
                    Weight=0,
                    Address="",
                    DelStatusId=(int)OrderStatusEnum.NotPlaced

                };
                await _uow.Orders.AddOrderAsync(order);
                await _uow.Save();
                order = await _uow.Orders.GetCartAsync(clientId);

            }
            return MapToCartDto(order);
        }

        public async Task<int> GetOrCreateCartIdAsync(string clientId)
        {
            var cart = await GetOrCreateCartAsync(clientId);
            return cart.Id;
        }

        public async Task<CartDto> UpdateCartAsync(CartDto cartDto)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var order = await _uow.Orders.GetOrderByIdAsync(cartDto.Id);
                if (order == null)
                    throw new ArgumentException("Корзина не найдена");

                order.Address = cartDto.Address;
                var dtoItemIds = cartDto.Items.Where(i => i.Id != 0).Select(i => i.Id).ToHashSet();

                foreach (var itemDto in cartDto.Items)
                {
                    OrderLine orderLine;


                    orderLine = order.OrderLines.FirstOrDefault(ol => ol.Id == itemDto.Id);

                    var pizza = await _uow.Pizzas.GetPizzaByIdAsync(itemDto.PizzaId);

                    orderLine.Pizza = pizza;

                    orderLine.PizzaId = itemDto.PizzaId;
                    orderLine.Quantity = itemDto.Quantity;

                    var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByNameAsync(itemDto.PizzaSize);
                    orderLine.PizzaSizeId = pizzaSize.Id;

                    orderLine.Ingredients.Clear();
                    foreach (var ingredientId in itemDto.AddedIngredientIds)
                    {
                        var ingredient = await _uow.Ingredients.GetIngredientByIdAsync(ingredientId);

                        orderLine.Ingredients.Add(ingredient);
                    }

                    orderLine.Price = pizzaSize.Price + orderLine.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.PricePerGram * i.Small :
                                                                                    pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.PricePerGram * i.Medium :
                                                                                    i.PricePerGram * i.Big)
                        + orderLine.Pizza.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.PricePerGram * i.Small :
                                                                                    pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.PricePerGram * i.Medium :
                                                                                    i.PricePerGram * i.Big);

                    orderLine.Weight = pizzaSize.Weight + orderLine.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.Small :
                                                                                    pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.Medium :
                                                                                    i.Big)
                        + orderLine.Pizza.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.Small :
                                                                                    pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.Medium :
                                                                                    i.Big);

                }
                var linesToRemove = order.OrderLines
                    .Where(ol => !dtoItemIds.Contains(ol.Id))
                    .ToList();
                foreach (var line in linesToRemove)
                {
                    order.OrderLines.Remove(line);
                    _uow.OrderLines.DeleteOrderLineAsync(line.Id);
                }

                order.Price = order.OrderLines.Sum(ol => ol.Price * ol.Quantity);
                order.Weight = order.OrderLines.Sum(ol => ol.Weight * ol.Quantity);

                //await _uow.Save();
                await _uow.CommitTransactionAsync();

                return MapToCartDto(order);
            }
            catch (Exception ex)
            {
               await  _uow.RollbackTransactionAsync();
               throw new Exception();
            }
        }

        public CartDto MapToCartDto(Order order)
        {
            CartDto cart = new CartDto
            {
                Id = order.Id,
                ClientId = order.ClientId,
                Address = order.Address,
                TotalPrice=order.Price,
                TotalWeight=order.Weight
            };
            List<CartItemDto> items = order.OrderLines.Select(
                ol => new CartItemDto
                {
                    Id = ol.Id,
                    PizzaId = ol.PizzaId,
                    PizzaName = ol.Pizza.Name,
                    PizzaImage = ol.Pizza.Image,
                    PizzaSize = ol.PizzaSize.Name,
                    PizzaSizeId = ol.PizzaSize.Id,
                    ItemPrice = ol.Price,
                    ItemWeight = ol.Weight,
                    Quantity = ol.Quantity,
                    DefaultIngredientIds = ol.Pizza.Ingredients.Select(i=>i.Id).ToList(),
                    AddedIngredientIds = ol.Ingredients.Select(i=>i.Id).ToList()
                }).ToList();
            cart.Items = items;
            return cart;

        }

        // Вспомогательные методы для расчетов
        private decimal CalculatePrice(Pizza pizza, Domain.Entities.PizzaSize size, List<Ingredient> ingredients)
        {
           

            decimal ingredientsPrice = ingredients.Sum(i =>
                i.PricePerGram * GetSizeValue(i, size.Id)
            ) + pizza.Ingredients.Sum(i =>
            i.PricePerGram * GetSizeValue(i, size.Id));

            return size.Price + ingredientsPrice;
        }

        private decimal CalculateWeight(Pizza pizza, Domain.Entities.PizzaSize size, List<Ingredient> ingredients)
        {
            

            decimal ingredientsWeight = ingredients.Sum(i =>
                GetSizeValue(i, size.Id)
            ) + pizza.Ingredients.Sum(i => GetSizeValue(i, size.Id));

            return size.Weight + ingredientsWeight;
        }

        private decimal GetSizeValue(Ingredient ingredient, int sizeId)
        {
            return sizeId switch
            {
                (int)Domain.Entities.PizzaSizeEnum.Small => ingredient.Small,
                (int)Domain.Entities.PizzaSizeEnum.Medium => ingredient.Medium,
                (int)Domain.Entities.PizzaSizeEnum.Big => ingredient.Big,
                _ => 0
            };
        }

        public async Task<CartDto> AddNewItemToCartAsync(NewCartItemDto itemDto)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(itemDto.CartId);

            var pizza = await _uow.Pizzas.GetPizzaByIdAsync(itemDto.PizzaId);
            //var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByNameAsync(itemDto.PizzaSize);
            var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByIdAsync(itemDto.PizzaSizeId);

            var ingredients = new List<Ingredient>();
            foreach(var ingredientId in itemDto.AddedIngredientIds)
            {
                var ingredient = await _uow.Ingredients.GetIngredientByIdAsync(ingredientId);
                ingredients.Add(ingredient);
            }

            var orderLine = new OrderLine
            {
                Id = 0,
                PizzaId = itemDto.PizzaId,
                PizzaSizeId = pizzaSize.Id,
                Quantity = itemDto.Quantity,
                Ingredients = ingredients,
                Price = CalculatePrice(pizza, pizzaSize, ingredients),
                Weight = CalculateWeight(pizza, pizzaSize, ingredients),
                Custom=ingredients.Count()>0
            };
            order.OrderLines.Add(orderLine);
            order.Price += orderLine.Price * orderLine.Quantity;
            order.Weight += orderLine.Weight * orderLine.Quantity;

            await _uow.Save();

            _logger.LogInformation("Добавлена новая позиция в корзину {CartId}", itemDto.CartId);

            order = await _uow.Orders.GetOrderByIdAsync(itemDto.CartId);


            return MapToCartDto(order);
        }

        public async Task SubmitCartAsync(string clientId, decimal price, string address)
        {
            _logger.LogInformation("Начало оформления корзины для пользователя {ClientId}, clientId");

            await _uow.BeginTransactionAsync();
            try
            {
                var cart = await _uow.Orders.GetCartAsync(clientId);
                if (cart == null)
                {
                    _logger.LogWarning("Корзина для пользователя {ClientId} не найдена", clientId);
                    throw new Exception("Корзина не найдена");
                }

                if(cart.Price!=price)
                {
                    _logger.LogWarning("Корзина клиента устарела");
                    throw new Exception("Корзина клиента устарела");
                }
                //if (string.IsNullOrWhiteSpace(cart.Address))
                //{
                //    _logger.LogWarning("Корзина для пользователя {ClientId}, адрес при заказе не указан", clientId);
                //    throw new Exception("Адрес доставки не указан");
                //}
                if (!cart.OrderLines.Any())
                {
                    _logger.LogWarning("Попытка оформления пустой корзины");
                    throw new Exception("Корзина пуста");
                }

                cart.DelStatusId = (int)OrderStatusEnum.IsBeingFormed;
                cart.OrderTime = DateTime.UtcNow;
                cart.Address = address;
                await _uow.Save();
                await _uow.CommitTransactionAsync();

                _logger.LogInformation("По корзине {CartId} успешно оформлен заказ", cart.Id);
                
            }
            catch(Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }


        public async Task<CartDto> RemoveItemFromCartAsync(int itemId)
        {
            var orderLine = await _uow.OrderLines.GetOrderLineByIdAsync(itemId);
            if (orderLine == null)
                throw new Exception("Такой позиции заказа нет");
            decimal price = orderLine.Price;
            decimal weight = orderLine.Weight;
            int quantity = orderLine.Quantity;
            var order = await _uow.Orders.GetOrderByIdAsync(orderLine.OrderId);
            await _uow.OrderLines.DeleteOrderLineAsync(orderLine.Id);

            order.Price -= price * quantity;
            order.Weight -= weight * quantity;
            await _uow.Save();
            order = await _uow.Orders.GetOrderByIdAsync(order.Id);
            return MapToCartDto(order);
            
        }

        public async Task<CartDto> UpdateItemFromcartAsync(NewCartItemDto itemDto)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(itemDto.CartId);

            var orderLine = order.OrderLines.Where(ol => ol.Id == itemDto.Id).FirstOrDefault();
            if (orderLine == null)
                throw new Exception("Такой позиции заказа нет");
            decimal price = orderLine.Price;
            decimal weight = orderLine.Weight;
            int quantity = orderLine.Quantity;
            
            //await _uow.OrderLines.DeleteOrderLineAsync(orderLine.Id);

            var pizza = await _uow.Pizzas.GetPizzaByIdAsync(itemDto.PizzaId);
            var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByIdAsync(itemDto.PizzaSizeId);

            var ingredients = new List<Ingredient>();
            foreach(var ingredientId in itemDto.AddedIngredientIds)
            {
                var ingredient = await _uow.Ingredients.GetIngredientByIdAsync(ingredientId);
                ingredients.Add(ingredient);
            }



            order.Price -= price * quantity;
            order.Weight -= weight * quantity;

            orderLine.PizzaId = itemDto.PizzaId;
            orderLine.PizzaSizeId = pizzaSize.Id;
            orderLine.Ingredients = ingredients;
            orderLine.Quantity = itemDto.Quantity;
            orderLine.Price = CalculatePrice(pizza, pizzaSize, ingredients);
            orderLine.Weight = CalculateWeight(pizza, pizzaSize, ingredients);

            order.Price += orderLine.Price * orderLine.Quantity;
            order.Weight += orderLine.Price * orderLine.Quantity;

            await _uow.Save();
            order = await _uow.Orders.GetOrderByIdAsync(order.Id);
            return MapToCartDto(order);
        }
    

    }
}
