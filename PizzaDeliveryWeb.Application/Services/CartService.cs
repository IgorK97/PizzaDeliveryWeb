using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaDeliveryWeb.Application.MyExceptions;
using Microsoft.EntityFrameworkCore;

namespace PizzaDeliveryWeb.Application.Services
{
    /// <summary>
    /// Сервис для работы с корзиной клиента.
    /// </summary>
    public class CartService
    {
        private readonly IUnitOfWork _uow;
        //private readonly ILogger<CartService> _logger;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CartService"/>.
        /// </summary>
        /// <param name="uow">Единица работы (Unit of Work) для доступа к данным.</param>

        public CartService(IUnitOfWork uow)
        {
            _uow = uow;
            //_logger = logger;
        }
        /// <summary>
        /// Получает существующую корзину по идентификатору клиента или создает новую, если корзина отсутствует.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Объект <see cref="CartDto"/> с данными корзины.</returns>
        public async Task<CartDto> GetOrCreateCartAsync(string clientId)
        {

            Order order;
            try
            {
                order = await _uow.Orders.GetCartAsync(clientId);
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при получении корзины. попробуйте позже", ex);
            }
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
                try
                {
                    await _uow.Orders.AddOrderAsync(order);
                    await _uow.Save();
                }
                catch(DbUpdateException ex)
                {
                    throw new MyDbException("Ошибка при сохранении корзины. Попробуйте позже.", ex);
                }

                try
                {
                    order = await _uow.Orders.GetCartAsync(clientId);
                }
                catch (DbUpdateException ex)
                {
                    throw new MyDbException("Ошибка при получении корзины. попробуйте позже", ex);
                }
                if (order == null)
                    throw new CartNotFoundException($"Не удалось получить корзину для клиента {clientId}");

            }
            return MapToCartDto(order);
        }
        /// <summary>
        /// Получает идентификатор корзины клиента, создавая её при необходимости.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Идентификатор корзины.</returns>
        public async Task<int> GetOrCreateCartIdAsync(string clientId)
        {
            var cart = await GetOrCreateCartAsync(clientId);
            return cart.Id;
        }

        //public async Task<CartDto> UpdateCartAsync(CartDto cartDto)
        //{
        //    //await _uow.BeginTransactionAsync();
        //    try
        //    {
        //        var order = await _uow.Orders.GetOrderByIdAsync(cartDto.Id);
        //        if (order == null)
        //            throw new ArgumentException("Корзина не найдена");

        //        order.Address = cartDto.Address;
        //        var dtoItemIds = cartDto.Items.Where(i => i.Id != 0).Select(i => i.Id).ToHashSet();

        //        foreach (var itemDto in cartDto.Items)
        //        {
        //            OrderLine orderLine;


        //            orderLine = order.OrderLines.FirstOrDefault(ol => ol.Id == itemDto.Id);

        //            var pizza = await _uow.Pizzas.GetPizzaByIdAsync(itemDto.PizzaId);

        //            orderLine.Pizza = pizza;

        //            orderLine.PizzaId = itemDto.PizzaId;
        //            orderLine.Quantity = itemDto.Quantity;

        //            var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByNameAsync(itemDto.PizzaSize);
        //            orderLine.PizzaSizeId = pizzaSize.Id;

        //            orderLine.Ingredients.Clear();
        //            foreach (var ingredientId in itemDto.AddedIngredientIds)
        //            {
        //                var ingredient = await _uow.Ingredients.GetIngredientByIdAsync(ingredientId);

        //                orderLine.Ingredients.Add(ingredient);
        //            }

        //            orderLine.Price = pizzaSize.Price + orderLine.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.PricePerGram * i.Small :
        //                                                                            pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.PricePerGram * i.Medium :
        //                                                                            i.PricePerGram * i.Big)
        //                + orderLine.Pizza.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.PricePerGram * i.Small :
        //                                                                            pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.PricePerGram * i.Medium :
        //                                                                            i.PricePerGram * i.Big);

        //            orderLine.Weight = pizzaSize.Weight + orderLine.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.Small :
        //                                                                            pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.Medium :
        //                                                                            i.Big)
        //                + orderLine.Pizza.Ingredients.Sum(i => pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Small ? i.Small :
        //                                                                            pizzaSize.Id == (int)Domain.Entities.PizzaSizeEnum.Medium ? i.Medium :
        //                                                                            i.Big);

        //        }
        //        var linesToRemove = order.OrderLines
        //            .Where(ol => !dtoItemIds.Contains(ol.Id))
        //            .ToList();
        //        foreach (var line in linesToRemove)
        //        {
        //            order.OrderLines.Remove(line);
        //            _uow.OrderLines.DeleteOrderLineAsync(line.Id);
        //        }

        //        order.Price = order.OrderLines.Sum(ol => ol.Price * ol.Quantity);
        //        order.Weight = order.OrderLines.Sum(ol => ol.Weight * ol.Quantity);

        //        //await _uow.Save();
        //        //await _uow.CommitTransactionAsync();

        //        return MapToCartDto(order);
        //    }
        //    catch (Exception ex)
        //    {
        //       //await  _uow.RollbackTransactionAsync();
        //       throw new Exception();
        //    }
        //}

        /// <summary>
        /// Преобразует сущность заказа в DTO корзины.
        /// </summary>
        /// <param name="order">Сущность заказа.</param>
        /// <returns>DTO корзины <see cref="CartDto"/>.</returns>
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

        /// <summary>
        /// Вычисляет цену пиццы с учетом добавленных ингредиентов и размера.
        /// </summary>
        /// <param name="pizza">Пицца.</param>
        /// <param name="size">Размер пиццы.</param>
        /// <param name="ingredients">Список добавленных ингредиентов.</param>
        /// <returns>Цена позиции.</returns>
        private decimal CalculatePrice(Pizza pizza, Domain.Entities.PizzaSize size, List<Ingredient> ingredients)
        {
           

            decimal ingredientsPrice = ingredients.Sum(i =>
                i.PricePerGram * GetSizeValue(i, size.Id)
            ) + pizza.Ingredients.Sum(i =>
            i.PricePerGram * GetSizeValue(i, size.Id));

            return size.Price + ingredientsPrice;
        }

        /// <summary>
        /// Вычисляет вес пиццы с учетом ингредиентов и размера.
        /// </summary>
        /// <param name="pizza">Пицца.</param>
        /// <param name="size">Размер пиццы.</param>
        /// <param name="ingredients">Список добавленных ингредиентов.</param>
        /// <returns>Вес позиции.</returns>
        private decimal CalculateWeight(Pizza pizza, Domain.Entities.PizzaSize size, List<Ingredient> ingredients)
        {
            

            decimal ingredientsWeight = ingredients.Sum(i =>
                GetSizeValue(i, size.Id)
            ) + pizza.Ingredients.Sum(i => GetSizeValue(i, size.Id));

            return size.Weight + ingredientsWeight;
        }

        /// <summary>
        /// Получает вес или стоимость ингредиента в зависимости от размера пиццы.
        /// </summary>
        /// <param name="ingredient">Ингредиент.</param>
        /// <param name="sizeId">Идентификатор размера.</param>
        /// <returns>Значение веса/цены.</returns>
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

        /// <summary>
        /// Добавляет новый товар в корзину.
        /// </summary>
        /// <param name="itemDto">Данные новой позиции корзины.</param>
        /// <returns>Обновленная корзина <see cref="CartDto"/>.</returns>
        /// <exception cref="ArgumentException">Если количество товара не положительное.</exception>
        /// <exception cref="NotFoundException">Если заказ, пицца или ингредиенты не найдены.</exception>

        public async Task<CartDto> AddNewItemToCartAsync(NewCartItemDto itemDto)
        {
            if (itemDto.Quantity <= 0)
                throw new ArgumentException("Количество товаров в позиции заказа должно быть больше нуля");

            var order = await _uow.Orders.GetOrderByIdAsync(itemDto.CartId);
            if (order == null)
                throw new NotFoundException("Корзина", itemDto.CartId);


            var pizza = await _uow.Pizzas.GetPizzaByIdAsync(itemDto.PizzaId);
            if (pizza == null)
                throw new NotFoundException("Пицца", itemDto.PizzaId);
            //var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByNameAsync(itemDto.PizzaSize);
            var pizzaSize = await _uow.PizzaSizes.GetPizzaSizeByIdAsync(itemDto.PizzaSizeId);
            if (pizzaSize == null)
                throw new NotFoundException("Размер пиццы", itemDto.PizzaSizeId);

            var ingredients = new List<Ingredient>();
            foreach(var ingredientId in itemDto.AddedIngredientIds)
            {
                var ingredient = await _uow.Ingredients.GetIngredientByIdAsync(ingredientId);
                if (ingredient == null)
                    throw new NotFoundException("Ингредиент", ingredientId);
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
            try
            {
                await _uow.Save();
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при сохранении корзины. попробуйте позже.", ex);
            }

            //_logger.LogInformation("Добавлена новая позиция в корзину {CartId}", itemDto.CartId);

            order = await _uow.Orders.GetOrderByIdAsync(itemDto.CartId);
            if (order == null)
                throw new NotFoundException("Корзина", itemDto.CartId);

            return MapToCartDto(order);
        }

        /// <summary>
        /// Подтверждает корзину, формируя заказ.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="price">Итоговая цена.</param>
        /// <param name="address">Адрес доставки.</param>
        /// <exception cref="ArgumentException">Если цена некорректна или адрес пуст.</exception>
        /// <exception cref="CartNotFoundException">Если корзина клиента не найдена.</exception>
        /// <exception cref="OutdatedCartException">Если данные корзины устарели.</exception>
        /// <exception cref="EmptyCartException">Если корзина пуста.</exception>

        public async Task SubmitCartAsync(string clientId, decimal price, string address)
        {

            if (price <= 0)
                throw new ArgumentException("Недопустимая цена");
            if (String.IsNullOrEmpty(address))
                throw new ArgumentException("Адрес должен быть указан");
            var cart = await _uow.Orders.GetCartAsync(clientId);
            if (cart == null)
                throw new CartNotFoundException($"Корзина для клиента {clientId} не найдена");
                
                if(cart.Price!=price)
                {

                    throw new OutdatedCartException("Корзина клиента устарела");
                }

                if (!cart.OrderLines.Any())
                {

                    throw new EmptyCartException("Корзина пуста");
                }

                cart.DelStatusId = (int)OrderStatusEnum.IsBeingFormed;
                cart.OrderTime = DateTime.UtcNow;
                cart.Address = address;
            try
            {
                await _uow.Save();
            }
            catch (DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при сохранении заказа. Попробуйте позже.", ex);
            }



        }

        /// <summary>
        /// Удаляет позицию из корзины по её идентификатору.
        /// </summary>
        /// <param name="itemId">Идентификатор позиции заказа.</param>
        /// <returns>Обновленная корзина <see cref="CartDto"/>.</returns>
        /// <exception cref="Exception">Если позиция не найдена.</exception>
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

        /// <summary>
        /// Обновляет существующую позицию в корзине.
        /// </summary>
        /// <param name="itemDto">Новые данные позиции корзины.</param>
        /// <returns>Обновленная корзина <see cref="CartDto"/>.</returns>
        /// <exception cref="Exception">Если позиция не найдена.</exception>
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
