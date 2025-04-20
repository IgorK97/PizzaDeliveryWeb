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

        private DeliveryDto MapToDeliveryDto(Delivery delivery)
        {
            return new DeliveryDto
            {
                Id = delivery.Id,
                AcceptanceTime = delivery.AcceptanceTime,
                DeliveryTime = delivery.DeliveryTime,
                Comment = delivery.Comment,
                CourierName = FormatName(delivery.Courier),
                IsSuccessful = delivery.IsSuccessful,
                Order = MapToOrderDto(delivery.Order)
            };
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                ClientName = FormatName(order.Client),
                FinalPrice = order.Price,
                Status = ((OrderStatusEnum)order.DelStatusId).ToString(),
                OrderTime = order.OrderTime,
                Address = order.Address,
                AcceptedTime = order.AcceptedTime,
                CancellationTime = order.CancellationTime,
                CompletionTime = order.CompletionTime,
                ClientId = order.ClientId,
                Weight = order.Weight,
                OrderLines = order.OrderLines?
               .Select(ol => new OrderLineShortDto
               {
                   Id = ol.Id,
                   PizzaId = ol.PizzaId,
                   PizzaName = ol.Pizza.Name,
                   Size = ol.PizzaSize.Name,
                   Quantity = ol.Quantity,
                   Price = ol.Price,
                   AddedIngredients = ol.Ingredients?
                   .Select(ai => ai.Name)
                   .ToList() ?? new List<string>()
               })
               .ToList() ?? new List<OrderLineShortDto>()
            };
        }

        private string FormatName(User client)
        {
            return $"{client.FirstName} {client.LastName}" +
                (!string.IsNullOrEmpty(client.Surname) ? $"{client.Surname}" : "");
        }




        public async Task<IEnumerable<DeliveryDto>> GetDeliveriesAsync()
        {
            var deliveries = await _deliveryRepository.GetDeliveriesAsync();
            return deliveries.Select(i=>MapToDeliveryDto(i));
        }

        public async Task<DeliveryDto> GetDeliveryByIdAsync(int deliveryId)
        {
            var delivery = await _deliveryRepository.GetDeliveryByIdAsync(deliveryId);
            return MapToDeliveryDto(delivery);

        }
        //Хотя для одного заказа нужна всего одна доставка, может произойти непредвиденная ситуация,
        //Из-за чего может быть проведена повторная доставка в рамках одного заказа
        //public async Task<IEnumerable<DeliveryDto>> GetDeliveriesByOrderIdAsync(int orderId)
        //{
        //    var deliveries = await _deliveryRepository.GetDeliveriesByOrderIdAsync(orderId);
        //    return deliveries.Select(i => MapToDeliveryDto(i));
        //}

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

      
    }
}
