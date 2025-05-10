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

    /// <summary>
    /// Сервис для упралвения ингредиентами (создание, получение, обновление, логическое удаление)
    /// </summary>
    public class IngredientService
    {
        private readonly IIngredientRepository _ingrRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPizzaSizeRepository _pizzaSizeRepository;
        private readonly IOrderLineRepository _orderLineRepository;


        /// <summary>
        /// инициализация нового экземпляра сервиса ингредиентов
        /// </summary>
        /// <param name="taskRepository"></param>
        /// <param name="orderRepository"></param>
        /// <param name="pizzaSizeRepository"></param>
        /// <param name="orderLineRepository"></param>
        public IngredientService(IIngredientRepository taskRepository, IOrderRepository orderRepository,
            IPizzaSizeRepository pizzaSizeRepository, IOrderLineRepository orderLineRepository)
        {
            _ingrRepository = taskRepository;
            _orderRepository = orderRepository;
            _pizzaSizeRepository = pizzaSizeRepository;
            _orderLineRepository = orderLineRepository;
        }

        /// <summary>
        /// Получает список всех доступных ингредиентов
        /// </summary>
        /// <returns>Коллекция DTO объектов ингредиентов</returns>
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

        /// <summary>
        /// Получает ингредиент по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор ингредиента</param>
        /// <returns>DTO объекта ингредиента или null, если не найден</returns>
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

        /// <summary>
        /// Добавляет новый ингредиент в систему
        /// </summary>
        /// <param name="ingrDto">DTO с данными для создания ингредиента</param>
        /// <returns>Созданный DTO объект ингредиента</returns>

        public async Task<IngredientDto> AddIngredientAsync(CreateIngredientDto ingrDto)
        {
            var t = new Ingredient
            {
                Id=0,
                Name = ingrDto.Name,
                Description = ingrDto.Description,
                Small = ingrDto.Small,
                Medium = ingrDto.Medium,
                Big = ingrDto.Big,
                PricePerGram = ingrDto.PricePerGram,
                Image=ingrDto.Image,
                IsAvailable=ingrDto.IsAvailable
            };
            await _ingrRepository.AddIngredientAsync(t);
            return new IngredientDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Small = t.Small,
                Medium = t.Medium,
                Big = t.Big,
                PricePerGram = t.PricePerGram,
                Image = t.Image,
                IsAvailable = t.IsAvailable
                
            };
            //ingrDto.Id = t.Id;
        }


        /// <summary>
        /// Обновляет существующий ингредиент
        /// </summary>
        /// <param name="ingrDto">DTO с обновленными данными ингредиента</param>
        /// <returns>Обновленный DTO объект ингредиента</returns>
        public async Task<IngredientDto> UpdateIngredientAsync(UpdateIngredientDto ingrDto)
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
                res.IsAvailable = ingrDto.IsAvailable;
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


                return new IngredientDto
                {
                    Id = res.Id,
                    Image = res.Image,
                    Big = res.Big,
                    Small = res.Small,
                    Medium = res.Medium,
                    Description = res.Description,
                    IsAvailable = res.IsAvailable,
                    Name = res.Name,
                    PricePerGram = res.PricePerGram
                };
            }
            throw new Exception("Ингредиента с таким id не существует");
        }


        /// <summary>
        /// Пересчитывает стоимость и вес позиции заказа
        /// </summary>
        /// <param name="orderLine">Позиция заказа для пересчета</param>
        /// <param name="ingredient">Измененный ингредиент</param>
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
                if (orderLine.PizzaSizeId == (int)Domain.Entities.PizzaSizeEnum.Small)
                {
                    elementWeight += oli.Small;
                    elementPrice += oli.PricePerGram * oli.Small;
                }
                else if (orderLine.PizzaSizeId == (int)Domain.Entities.PizzaSizeEnum.Medium)
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

        /// <summary>
        /// Удаляет ингредиент по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор удаляемого ингредиента</param>
        /// <exception cref="ArgumentException">Если ингредиент не найден</exception>
        /// <exception cref="InvalidOperationException">Если ингредиент используется в заказах</exception>

        public async Task DeleteIngredientAsync(int id)
        {
            await _ingrRepository.DeleteIngredientAsync(id);
        }
    }
}
