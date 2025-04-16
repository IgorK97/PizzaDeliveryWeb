using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class IngredientService
    {
        private readonly IIngredientRepository _ingrRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPizzaSizeRepository _pizzaSizeRepository;
        private readonly IOrderLineRepository _orderLineRepository;

        public IngredientService(IIngredientRepository taskRepository, IOrderRepository orderRepository,
            IPizzaSizeRepository pizzaSizeRepository, IOrderLineRepository orderLineRepository)
        {
            _ingrRepository = taskRepository;
            _orderRepository = orderRepository;
            _pizzaSizeRepository = pizzaSizeRepository;
            _orderLineRepository = orderLineRepository;
        }

        public async Task<IEnumerable<IngredientDto>> GetIngredientsAsync()
        {
            var oline = await _ingrRepository.GetIngredientsAsync();
            return oline.Select(t => new IngredientDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsAvailable=t.IsAvailable,
                Small = t.Small,
                Medium = t.Medium,
                Big = t.Big,
                PricePerGram = t.PricePerGram,
                Image=t.Image
            });
        }

        public async Task<IngredientDto> GetIngredientByIdAsync(int id)
        {
            var t = await _ingrRepository.GetIngredientByIdAsync(id);
            if (t == null) return null;
            return new IngredientDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Small = t.Small,
                Medium = t.Medium,
                Big = t.Big,
                PricePerGram = t.PricePerGram,
                Image=t.Image
            };
        }
           
        public async Task AddIngredientAsync(IngredientDto ingrDto)
        {
            var t = new Ingredient
            {
                Name = ingrDto.Name,
                Description = ingrDto.Description,
                Small = ingrDto.Small,
                Medium = ingrDto.Medium,
                Big = ingrDto.Big,
                PricePerGram = ingrDto.PricePerGram,
                Image=ingrDto.Image
            };
            await _ingrRepository.AddIngredientAsync(t);
            ingrDto.Id = t.Id;
        }

        public async Task UpdateIngredientAsync(IngredientDto ingrDto)
        {
            var res = await _ingrRepository.GetIngredientByIdAsync(ingrDto.Id);
            if (res != null)
            {
                res.Name = ingrDto.Name;
                res.Description = ingrDto.Description;
                res.Small = ingrDto.Small;
                res.Medium = ingrDto.Medium;
                res.Big = ingrDto.Big;
                res.PricePerGram = ingrDto.PricePerGram;
                res.Image = ingrDto.Image;
                await _ingrRepository.UpdateIngredientAsync(res);


                var affectedOrderLines = await _orderRepository
            .GetNotPlacedOrderLinesWithIngredientAsync(ingrDto.Id);

                foreach (var orderLine in affectedOrderLines)
                {
                    await RecalculateOrderLinePriceAsync(orderLine, res);
                    
                }

                var affectedOrders = affectedOrderLines
                    .Select(ol => ol.Order)
                    .Distinct()
                    .ToList();

                foreach (var order in affectedOrders)
                {
                    order.Price = order.OrderLines.Sum(ol => ol.Price);
                    order.Weight = order.OrderLines.Sum(ol => ol.Weight);
                    await _orderRepository.UpdateOrderAsync(order);
                }
            }
        }

        private async Task RecalculateOrderLinePriceAsync(OrderLine orderLine, Ingredient ingr)
        {
            //var ingredientsInLine = await _orderRepository
            //    .GetOrderLineIngredientsAsync(orderLine.Id);

            List<Domain.Entities.PizzaSize> ps = await _pizzaSizeRepository.GetPizzaSizesAsync();
            //Order order = await _orderRepository.GetOrderByIdAsync(orderLine.Id);
            //double totalWeight = orderLine.Pizza.BaseWeight; // Вес пиццы без ингредиентов
            //decimal totalPrice = orderLine.Pizza.BasePrice;  // Базовая цена пиццы
            
            decimal elementPrice = 0, elementWeight = 0;
            decimal oldPrice = orderLine.Price, oldWeight = orderLine.Weight;
            //order.Price -= oldPrice;
            //order.Weight -= oldWeight;
            foreach (var oli in orderLine.Ingredients)
            {
                //if (oli.Id == ingr.Id)
                //{
                //var currentIngredient = await _ingrRepository
                //.GetIngredientByIdAsync(oli.Id);
                if (orderLine.PizzaSizeId == (int)PizzaSizeEnum.Small)
                {
                    elementWeight += oli.Small;
                    elementPrice += oli.PricePerGram * oli.Small;
                }
                else if (orderLine.PizzaSizeId == (int)PizzaSizeEnum.Medium)
                {
                    elementWeight += oli.Medium;
                    elementPrice += oli.PricePerGram * oli.Medium;
                }
                else
                {
                    elementWeight += oli.Big;
                    elementPrice += oli.PricePerGram * oli.Big;
                }

                //}
            }
            elementPrice *= orderLine.Quantity;
            elementWeight *= orderLine.Quantity;
            orderLine.Weight = elementWeight;
            orderLine.Price = elementPrice;
            //order.Price += elementPrice;
            //order.Weight += elementWeight;

            await _orderLineRepository.UpdateOrderLineAsync(orderLine);
            //await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DeleteIngredientAsync(int id)
        {
            await _ingrRepository.DeleteIngredientAsync(id);
        }
    }
}
