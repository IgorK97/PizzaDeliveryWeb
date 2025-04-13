using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class OrderLineService
    {
        private readonly IOrderLineRepository _orderLineRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IPizzaRepository _pizzaRepository;
        private readonly IPizzaSizeRepository _pizzaSizeRepository;
        private readonly IOrderRepository _orderRepository;
        
        public OrderLineService(IOrderLineRepository orderLineRepository, IIngredientRepository ingredientRepository,
            IPizzaRepository pizzaRepository, IPizzaSizeRepository pizzaSizeRepository,
            IOrderRepository orderRepository)
        {
            _orderLineRepository = orderLineRepository;
            _ingredientRepository = ingredientRepository;
            _pizzaRepository = pizzaRepository;
            _pizzaSizeRepository = pizzaSizeRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderLineDto>> GetOrderLinesByOrderIdAsync(int orderId)
        {
            var result = await _orderLineRepository.GetOrderLinesByOrderIdAsync(orderId);
            return result.Select(ol =>
            {
                List<IngredientDto> ingrs = new List<IngredientDto>();
                List<IngredientDto> pizzaIngrs = new List<IngredientDto>();
                foreach (Ingredient i in ol.Pizza.Ingredients)
                    pizzaIngrs.Add(new IngredientDto
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Big = i.Big,
                        PricePerGram = i.PricePerGram,
                        Medium = i.Medium,
                        Name = i.Name,
                        Small = i.Small
                    });
                foreach (Ingredient i in ol.Ingredients)
                    ingrs.Add(new IngredientDto
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Big = i.Big,
                        PricePerGram = i.PricePerGram,
                        Medium = i.Medium,
                        Name = i.Name,
                        Small = i.Small
                    });
                return new OrderLineDto
                {
                    Id = ol.Id,
                    Price = ol.Price,
                    Weight = ol.Weight,
                    Quantity = ol.Quantity,
                    Size = ol.PizzaSize.Name,
                    OrderId = orderId,
                    Ingredients = ingrs,
                    Pizza = new PizzaDto
                    {
                        Id = ol.PizzaId,
                        Name = ol.Pizza.Name,
                        Description = ol.Pizza.Description,
                        Image = ol.Pizza.Image,
                        IsAvailable = ol.Pizza.IsAvailable,
                        Ingredients = pizzaIngrs
                    }
                };
            });
        }

        public async Task<OrderLineDto> GetOrderLineByIdAsync(int id)
        {
            var ol = await _orderLineRepository.GetOrderLineByIdAsync(id);

            List<IngredientDto> ingrs = new List<IngredientDto>();
            List<IngredientDto> pizzaIngrs = new List<IngredientDto>();
            foreach (Ingredient i in ol.Pizza.Ingredients)
                pizzaIngrs.Add(new IngredientDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Big = i.Big,
                    PricePerGram = i.PricePerGram,
                    Medium = i.Medium,
                    Name = i.Name,
                    Small = i.Small
                });
            foreach (Ingredient i in ol.Ingredients)
                ingrs.Add(new IngredientDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Big = i.Big,
                    PricePerGram = i.PricePerGram,
                    Medium = i.Medium,
                    Name = i.Name,
                    Small = i.Small
                });
            return new OrderLineDto
            {
                Id = ol.Id,
                Price = ol.Price,
                Weight = ol.Weight,
                Quantity = ol.Quantity,
                Size = ol.PizzaSize.Name,
                OrderId = ol.OrderId,
                Ingredients = ingrs,
                Pizza = new PizzaDto
                {
                    Id = ol.PizzaId,
                    Name = ol.Pizza.Name,
                    Description = ol.Pizza.Description,
                    Image = ol.Pizza.Image,
                    IsAvailable = ol.Pizza.IsAvailable,
                    Ingredients = pizzaIngrs
                }
            };

        }

        public async Task AddOrderLineAsync(CreateOrderLineDto oline)
        {
            decimal totalPrice = 0, totalWeight = 0, elementPrice = 0, elementWeight = 0;

            decimal basePrice = 0, baseWeight = 0;

            decimal innerPrice = 0, innerWeight = 0;

            decimal additionalPrice = 0, additionalWeight = 0;


            OrderLine orderLine = new OrderLine
            {
                Id = 0,
                Custom = oline.Custom,
                PizzaId = oline.PizzaId,
                OrderId = oline.OrderId,
                PizzaSizeId = oline.PizzaSizeId,
                //Price=elementPrice,
                //Weight=elementWeight,
                Quantity = oline.Quantity
            };
            List<Ingredient> lineIngrs = new List<Ingredient>();
            foreach(int id in oline.AddedIngredients)
            {
                Ingredient ingr = await _ingredientRepository.GetIngredientByIdAsync(id);
                lineIngrs.Add(ingr);
            }
            Pizza pizza = await _pizzaRepository.GetPizzaByIdAsync(oline.PizzaId);

            if (oline.PizzaSizeId == (int)PizzaSizeEnum.Small)
            {
                additionalPrice = lineIngrs.Sum(i => i.Small * i.PricePerGram);
                additionalWeight = lineIngrs.Sum(i => i.Small);

                innerPrice = pizza.Ingredients.Sum(i => i.Small * i.PricePerGram);
                innerWeight = pizza.Ingredients.Sum(i => i.Small);
            }
            else if (oline.PizzaSizeId == (int)PizzaSizeEnum.Medium)
            {
                additionalPrice = lineIngrs.Sum(i => i.Medium * i.PricePerGram);
                additionalWeight = lineIngrs.Sum(i => i.Medium);

                innerPrice = pizza.Ingredients.Sum(i => i.Medium * i.PricePerGram);
                innerWeight = pizza.Ingredients.Sum(i => i.Medium);
            }
            else
            {
                additionalWeight = lineIngrs.Sum(i => i.Big);
                additionalPrice = lineIngrs.Sum(i => i.Big * i.PricePerGram);

                innerPrice = pizza.Ingredients.Sum(i => i.Big * i.PricePerGram);
                innerWeight = pizza.Ingredients.Sum(i => i.Big);
            }

            PizzaSize ps = await _pizzaSizeRepository.GetPizzaSizeByIdAsync(oline.PizzaSizeId);
            basePrice = ps.Price;
            baseWeight = ps.Weight;

            elementPrice = innerPrice + additionalPrice + basePrice;
            elementWeight = innerWeight + additionalWeight + baseWeight;

            elementWeight *= oline.Quantity;
            elementPrice *= oline.Quantity;

            orderLine.Price = elementPrice;
            orderLine.Weight = elementWeight;
            await _orderLineRepository.AddOrderLineAsync(orderLine);

            Order order = await _orderRepository.GetOrderByIdAsync(oline.OrderId);
            order.Price += elementPrice;
            order.Weight += elementWeight;
            await _orderRepository.UpdateOrderAsync(order);
        }
        public async Task UpdateOrderLineAsync(CreateOrderLineDto oline)
        {
            OrderLine orderLine = await _orderLineRepository.GetOrderLineByIdAsync(oline.Id);
            if (orderLine == null || orderLine.Order.DelStatusId!=(int)OrderStatusEnum.NotPlaced)
            {
                throw new Exception("Заказ и его содержимое не могут быть изменены");
            }

            decimal oldPrice = orderLine.Price, oldWeight = orderLine.Weight, elementPrice = 0, elementWeight = 0;

            decimal basePrice = 0, baseWeight = 0;

            decimal innerPrice = 0, innerWeight = 0;

            decimal additionalPrice = 0, additionalWeight = 0;


            orderLine.Custom = oline.Custom;
            orderLine.PizzaId = oline.PizzaId;
            orderLine.OrderId = oline.OrderId;
            orderLine.PizzaSizeId = oline.PizzaSizeId;
            //Price=elementPrice,
            //Weight=elementWeight,
            orderLine.Quantity = oline.Quantity;
            
            List<Ingredient> lineIngrs = new List<Ingredient>();
            foreach (int id in oline.AddedIngredients)
            {
                Ingredient ingr = await _ingredientRepository.GetIngredientByIdAsync(id);
                lineIngrs.Add(ingr);
            }
            Pizza pizza = await _pizzaRepository.GetPizzaByIdAsync(oline.PizzaId);

            if (oline.PizzaSizeId == (int)PizzaSizeEnum.Small)
            {
                additionalPrice = lineIngrs.Sum(i => i.Small * i.PricePerGram);
                additionalWeight = lineIngrs.Sum(i => i.Small);

                innerPrice = pizza.Ingredients.Sum(i => i.Small * i.PricePerGram);
                innerWeight = pizza.Ingredients.Sum(i => i.Small);
            }
            else if (oline.PizzaSizeId == (int)PizzaSizeEnum.Medium)
            {
                additionalPrice = lineIngrs.Sum(i => i.Medium * i.PricePerGram);
                additionalWeight = lineIngrs.Sum(i => i.Medium);

                innerPrice = pizza.Ingredients.Sum(i => i.Medium * i.PricePerGram);
                innerWeight = pizza.Ingredients.Sum(i => i.Medium);
            }
            else
            {
                additionalWeight = lineIngrs.Sum(i => i.Big);
                additionalPrice = lineIngrs.Sum(i => i.Big * i.PricePerGram);

                innerPrice = pizza.Ingredients.Sum(i => i.Big * i.PricePerGram);
                innerWeight = pizza.Ingredients.Sum(i => i.Big);
            }

            PizzaSize ps = await _pizzaSizeRepository.GetPizzaSizeByIdAsync(oline.PizzaSizeId);
            basePrice = ps.Price;
            baseWeight = ps.Weight;

            elementPrice = innerPrice + additionalPrice + basePrice;
            elementWeight = innerWeight + additionalWeight + baseWeight;

            elementWeight *= oline.Quantity;
            elementPrice *= oline.Quantity;

            orderLine.Price = elementPrice;
            orderLine.Weight = elementWeight;
            await _orderLineRepository.UpdateOrderLineAsync(orderLine);

            Order order = await _orderRepository.GetOrderByIdAsync(oline.OrderId);
            order.Price -= oldPrice;
            order.Weight -= oldWeight;

            order.Price += elementPrice;
            order.Weight += elementWeight;
            await _orderRepository.UpdateOrderAsync(order);

        }
        public async Task DeleteOrderLineAsync(int id)
        {
            try
            {
                await _orderLineRepository.DeleteOrderLineAsync(id);
            }
            catch (InvalidOperationException ex)
            {

                throw new ApplicationException($"Ошибка при удалении строки: {ex.Message}", ex);
            }
        }
    }
}
