using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class DeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IOrderRepository _orderRepository;
        public DeliveryService(IDeliveryRepository deliveryRepository, IOrderRepository orderRepository)
        {
            _deliveryRepository = deliveryRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<DeliveryDto>> GetDeliveriesAsync()
        {
            var deliveries = await _deliveryRepository.GetDeliveriesAsync();
            return deliveries.Select(d =>
            {

                List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                foreach (OrderLine ol in d.Order.OrderLines)
                {
                    List<IngredientDto> ingrs = new List<IngredientDto>();
                    foreach (Ingredient i in ol.Pizza.Ingredients)
                        ingrs.Add(new IngredientDto
                        {
                            Id = i.Id,
                            Big = i.Big,
                            Description = i.Description,
                            Medium = i.Medium,
                            Name = i.Name,
                            PricePerGram = i.PricePerGram,
                            Small = i.Small
                        });
                    oLinesDto.Add(new OrderLineDto
                    {
                        Id = ol.Id,
                        Price = ol.Price,
                        Quantity = ol.Quantity,
                        Weight = ol.Weight,
                        Size = ol.PizzaSize.Name,
                        Pizza = new PizzaDto
                        {
                            Id = ol.PizzaId,
                            Description = ol.Pizza.Description,
                            Image = ol.Pizza.Image,
                            IsAvailable = ol.Pizza.IsAvailable,
                            Name = ol.Pizza.Name,
                            Ingredients = ingrs
                        }
                    });
                }
                return new DeliveryDto
                {

                    Id = d.Id,
                    AcceptanceTime = d.AcceptanceTime,
                    DeliveryTime=d.DeliveryTime,
                    Comment=d.Comment,
                    CourierName=d.Courier.FirstName+d.Courier.LastName+(String.IsNullOrEmpty(d.Courier.Surname)?
                    "":d.Courier.Surname),
                    IsSuccessful=d.IsSuccessful,
                    Order=new OrderDto
                    {
                        Id=d.Order.Id,
                        Address=d.Order.Address,
                        AcceptedTime=d.Order.AcceptedTime,
                        CancellationTime=d.Order.CancellationTime,
                        ClientName=d.Order.Client.FirstName+d.Order.Client.LastName+
                        (String.IsNullOrEmpty(d.Order.Client.Surname)?"":d.Order.Client.Surname),
                        CompletionTime=d.Order.CompletionTime,
                        FinalPrice=d.Order.Price,
                        OrderTime=d.Order.OrderTime,
                        UserId=d.Order.ClientId,
                        Status=d.Order.DelStatus.Description,
                        Weight=d.Order.Weight,
                        OrderLines=oLinesDto
                    }
                };
            });
        }

        public async Task<DeliveryDto> GetDeliveryByIdAsync(int deliveryId)
        {
            var delivery = await _deliveryRepository.GetDeliveryByIdAsync(deliveryId);

            List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
            foreach (OrderLine ol in delivery.Order.OrderLines)
            {
                List<IngredientDto> ingrs = new List<IngredientDto>();
                foreach (Ingredient i in ol.Pizza.Ingredients)
                    ingrs.Add(new IngredientDto
                    {
                        Id = i.Id,
                        Big = i.Big,
                        Description = i.Description,
                        Medium = i.Medium,
                        Name = i.Name,
                        PricePerGram = i.PricePerGram,
                        Small = i.Small
                    });
                oLinesDto.Add(new OrderLineDto
                {
                    Id = ol.Id,
                    Price = ol.Price,
                    Quantity = ol.Quantity,
                    Weight = ol.Weight,
                    Size = ol.PizzaSize.Name,
                    Pizza = new PizzaDto
                    {
                        Id = ol.PizzaId,
                        Description = ol.Pizza.Description,
                        Image = ol.Pizza.Image,
                        IsAvailable = ol.Pizza.IsAvailable,
                        Name = ol.Pizza.Name,
                        Ingredients = ingrs
                    }
                });
            }
            return new DeliveryDto
            {

                Id = delivery.Id,
                AcceptanceTime = delivery.AcceptanceTime,
                DeliveryTime = delivery.DeliveryTime,
                Comment = delivery.Comment,
                CourierName = delivery.Courier.FirstName + delivery.Courier.LastName + (String.IsNullOrEmpty(delivery.Courier.Surname) ?
                "" : delivery.Courier.Surname),
                IsSuccessful = delivery.IsSuccessful,
                Order = new OrderDto
                {
                    Id = delivery.Order.Id,
                    Address = delivery.Order.Address,
                    AcceptedTime = delivery.Order.AcceptedTime,
                    CancellationTime = delivery.Order.CancellationTime,
                    ClientName = delivery.Order.Client.FirstName + delivery.Order.Client.LastName +
                    (String.IsNullOrEmpty(delivery.Order.Client.Surname) ? "" : delivery.Order.Client.Surname),
                    CompletionTime = delivery.Order.CompletionTime,
                    FinalPrice = delivery.Order.Price,
                    OrderTime = delivery.Order.OrderTime,
                    UserId = delivery.Order.ClientId,
                    Status = delivery.Order.DelStatus.Description,
                    Weight = delivery.Order.Weight,
                    OrderLines = oLinesDto
                }
            };

        }
        //Хотя для одного заказа нужна всего одна доставка, может произойти непредвиденная ситуация,
        //Из-за чего может быть проведена повторная доставка в рамках одного заказа
        public async Task<IEnumerable<DeliveryDto>> GetDeliveriesByOrderIdAsync(int orderId)
        {
            var deliveries = await _deliveryRepository.GetDeliveriesByOrderIdAsync(orderId);
            return deliveries.Select(d =>
            {

                List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                foreach (OrderLine ol in d.Order.OrderLines)
                {
                    List<IngredientDto> ingrs = new List<IngredientDto>();
                    foreach (Ingredient i in ol.Pizza.Ingredients)
                        ingrs.Add(new IngredientDto
                        {
                            Id = i.Id,
                            Big = i.Big,
                            Description = i.Description,
                            Medium = i.Medium,
                            Name = i.Name,
                            PricePerGram = i.PricePerGram,
                            Small = i.Small
                        });
                    oLinesDto.Add(new OrderLineDto
                    {
                        Id = ol.Id,
                        Price = ol.Price,
                        Quantity = ol.Quantity,
                        Weight = ol.Weight,
                        Size = ol.PizzaSize.Name,
                        Pizza = new PizzaDto
                        {
                            Id = ol.PizzaId,
                            Description = ol.Pizza.Description,
                            Image = ol.Pizza.Image,
                            IsAvailable = ol.Pizza.IsAvailable,
                            Name = ol.Pizza.Name,
                            Ingredients = ingrs
                        }
                    });
                }
                return new DeliveryDto
                {

                    Id = d.Id,
                    AcceptanceTime = d.AcceptanceTime,
                    DeliveryTime = d.DeliveryTime,
                    Comment = d.Comment,
                    CourierName = d.Courier.FirstName + d.Courier.LastName + (String.IsNullOrEmpty(d.Courier.Surname) ?
                    "" : d.Courier.Surname),
                    IsSuccessful = d.IsSuccessful,
                    Order = new OrderDto
                    {
                        Id = d.Order.Id,
                        Address = d.Order.Address,
                        AcceptedTime = d.Order.AcceptedTime,
                        CancellationTime = d.Order.CancellationTime,
                        ClientName = d.Order.Client.FirstName + d.Order.Client.LastName +
                        (String.IsNullOrEmpty(d.Order.Client.Surname) ? "" : d.Order.Client.Surname),
                        CompletionTime = d.Order.CompletionTime,
                        FinalPrice = d.Order.Price,
                        OrderTime = d.Order.OrderTime,
                        UserId = d.Order.ClientId,
                        Status = d.Order.DelStatus.Description,
                        Weight = d.Order.Weight,
                        OrderLines = oLinesDto
                    }
                };
            });
        }

        public async Task AddDeliveryAsync(CreateDeliveryDto createDeliveryDto)
        {
            Order order = await _orderRepository.GetOrderByIdAsync(createDeliveryDto.OrderId);
            if(order==null || order.DelStatus.Id != (int)OrderStatusEnum.IsBeingTransferred)
            {
                throw new Exception("Невозможно передать в доставку этот заказ");
            }
            Delivery delivery = new Delivery
            {
                Id = 0,
                AcceptanceTime = DateTime.UtcNow,
                Comment = null,
                CourierId = createDeliveryDto.CourierId,
                DeliveryTime = null,
                IsSuccessful = null,
                OrderId = createDeliveryDto.OrderId
            };
            await _deliveryRepository.AddDeliveryAsync(delivery);
        }
        //Удалять доставки не надо, надо либо их успешно завершить, либо провалить
        public async Task UpdateDeliveryAsync(UpdateDeliveryDto deliveryDto)
        {
            Delivery delivery = await _deliveryRepository.GetDeliveryByIdAsync(deliveryDto.Id);
            delivery.DeliveryTime = deliveryDto.DeliveryTime;
            delivery.IsSuccessful = deliveryDto.IsSuccessful;
            delivery.Comment = deliveryDto.Comment;
            await _deliveryRepository.UpdateDeliveryAsync(delivery);
        }

        //public async Task DeliverOrderAsync(int id)
        //{

        //}

        //public async Task BotchDeliveryAsync(int id)
        //{

        //}
    }
}
