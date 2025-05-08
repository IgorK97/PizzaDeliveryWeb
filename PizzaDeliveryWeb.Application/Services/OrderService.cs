using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.MyExceptions;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    
    public class OrderService
    {
       
        private readonly IUnitOfWork _uow;
        public OrderService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task CompleteDeliveryAsync(int orderId, string status, string comment)
        {
            var order = await _uow.Orders.GetOrderWithDeliveryAsync(orderId);
            if (order == null)
                throw new NotFoundException("Заказ", orderId);
            //Проверяем существование хотя бы одной доставки
            if (order.Delivery==null)
                throw new InvalidOperationException("Для заказа не создана доставка");
            var delivery = await _uow.Deliveries.GetDeliveryByOrderIdAsync(orderId);
            if (delivery == null)
                throw new NotFoundException("дотсавка для заказа", orderId);
            
            var deliveryStatus =await _uow.Statuses.GetStatusByDescriptionAsync(status);
            if (deliveryStatus == null)
                throw new ArgumentException("Ошибка при указании статуса");
            delivery.DeliveryTime = DateTime.UtcNow;
            if (deliveryStatus.Id == (int)OrderStatusEnum.IsDelivered)
            {
                delivery.IsSuccessful = true;
                order.DelStatusId = (int)OrderStatusEnum.IsDelivered;
            }
            else
            {
                delivery.IsSuccessful = false;
                delivery.Comment = comment;
                order.DelStatusId = (int)OrderStatusEnum.IsNotDelivered;

            }

            try
            {
                await _uow.Save();
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при сохранении информации о доставке", ex);
            }
        }

        public async Task<IEnumerable<ShortOrderDto>> GetOrdersByStatusAsync(
        OrderStatusEnum status,
        int? lastId,
        int? pageSize=10)
        {
            var orders = await _uow.Orders.GetOrdersByStatusAsync((int)status, lastId, pageSize);
            return orders.Select(MapToShortOrderDto);
        }

        public async Task TransferToDelivery(int orderId)
        {
            var order = await _uow.Orders.GetOrderWithDeliveryAsync(orderId);
            if (order == null)
                throw new NotFoundException("Заказ", orderId);
            if (order.DelStatusId != (int)OrderStatusEnum.IsBeingPrepared)
                throw new InvalidOperationException("Невозможно начать доставку");

            order.DelStatusId = (int)OrderStatusEnum.IsBeingTransferred;
            order.CompletionTime = DateTime.UtcNow;

            try
            {
                await _uow.Save();
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при обновлении заказа в процессе передачи в доставку", ex);
            }
        }
        public async Task TakeOrder(int orderId, string courierId)
        {
            var order = await _uow.Orders.GetOrderWithDeliveryAsync(orderId);
            if (order == null)
                throw new NotFoundException("Заказ", orderId);
            if (order.DelStatusId != (int)OrderStatusEnum.IsBeingTransferred)
                throw new InvalidOperationException("Невозможно начать доставку");


            var existingDelivery = await _uow.Deliveries.GetDeliveryByOrderIdAsync(orderId);
            if (existingDelivery != null)
                throw new InvalidOperationException("Доставка для этого заказа уже назначена");

            order.DelStatusId = (int)OrderStatusEnum.HasBeenTransferred;
            //order.CompletionTime = DateTime.UtcNow;
            var delivery = new Delivery
            {
                OrderId = orderId,
                AcceptanceTime = DateTime.UtcNow,
                CourierId=courierId
            };
            try
            {
                await _uow.Deliveries.AddDeliveryAsync(delivery);
                await _uow.Save();
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при сохранении данных о доставке", ex);
            }
        }

        private async Task<Order> ValidateOrderForAcceptance(int orderId)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new NotFoundException("Заказ", orderId);

            if (order.DelStatusId != (int)OrderStatusEnum.IsBeingFormed)
                throw new InvalidOperationException("Невозможно принять заказ в текущем статусе");

            return order;
        }

        public async Task AcceptOrderAsync(int orderId, string managerId)
        {
            //await _uow.BeginTransactionAsync();
            
                var order = await ValidateOrderForAcceptance(orderId);
                order.ManagerId = managerId;
                order.DelStatusId = (int)OrderStatusEnum.IsBeingPrepared;
                order.AcceptedTime = DateTime.UtcNow;
            try
            {
                await _uow.Save();
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при сохранении принятия заказа", ex);
            }
            
          
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                ClientName = FormatClientName(order.Client),
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
                   PizzaImage=ol.Pizza.Image,
                   Weight=ol.Weight,
                   Size = ol.PizzaSize.Name,
                   PizzaSizeId=(DTOs.PizzaSizeEnum)ol.PizzaSizeId,
                   Quantity = ol.Quantity,
                   Price = ol.Price,
                   AddedIngredients = ol.Ingredients?
                   .Select(ai => ai.Name)
                   .ToList() ?? new List<string>()
               })
               .ToList() ?? new List<OrderLineShortDto>()
            };
        }
        private ShortOrderDto MapToShortOrderDto(Order order)
        {
            return new ShortOrderDto
            {
                Id = order.Id,
                Price = order.Price,
                StatusId = (OrderStatusEnum) order.DelStatus.Id,
                OrderTime = (DateTime)order.OrderTime,
                ClientId = order.ClientId,
                CourierId = order.Delivery?.CourierId,
                Address = order.Address,
                AcceptedTime = order.AcceptedTime,
                CancellationTime = order.CancellationTime,
                EndCookingTime = order.CompletionTime,
                CompletionTime = order.Delivery != null ? order.Delivery.DeliveryTime : null,
                DeliveryStartTime = order.Delivery!=null?order.Delivery.AcceptanceTime:null,
                IsCancelled = order.CancellationTime != null,
                IsDelivered = order.Delivery!=null,
                OrderLines = order.OrderLines?
               .Select(ol => new OrderLineShortDto
               {
                   Id = ol.Id,
                   PizzaId = ol.PizzaId,
                   PizzaName = ol.Pizza.Name,
                   Size = ol.PizzaSize.Name,
                   PizzaSizeId=(DTOs.PizzaSizeEnum)ol.PizzaSizeId,
                   Quantity = ol.Quantity,
                   Price = ol.Price,
                   Weight = order.Weight,
                   PizzaImage = ol.Pizza.Image,

                   AddedIngredients = ol.Ingredients?
                   .Select(ai => ai.Name)
                   .ToList() ?? new List<string>()
               })
               .ToList() ?? new List<OrderLineShortDto>()
            };
        }



        private string FormatClientName(User client)
        {
            return $"{client.FirstName} {client.LastName}" +
                (!string.IsNullOrEmpty(client.Surname) ? $"{client.Surname}" : "");
        }

        

        public async Task<IEnumerable<ShortOrderDto>> GetClientOrderHistoryAsync(string userId, int? lastId=null, int? pageSize=10)
        {
            var orders = await _uow.Orders.GetOrdersByClientIdAsync(userId, lastId, pageSize);
            return orders.Select(MapToShortOrderDto);

            //return await GetOrdersByFilterAsync(
            //    o => o.ClientId == userId &&
            //         o.DelStatusId != (int)OrderStatusEnum.NotPlaced);
        }


        public async Task<IEnumerable<ShortOrderDto>> GetCourierActiveOrdersAsync(string courierId,
            OrderStatusEnum? filterStatus, int? lastId = null, int? pageSize=10)
        {
            if(filterStatus==null)
            {
                var allOrders = await _uow.Orders.GetAllOrdersForCourier(courierId, lastId, pageSize);
                return allOrders.Select(MapToShortOrderDto);
            }
            var orders = filterStatus != OrderStatusEnum.IsBeingTransferred ?
                await _uow.Orders.GetOrdersByStatusAsync((int)filterStatus, lastId, pageSize) :
                await _uow.Orders.GetOrdersByCourierIdAsync(courierId, filterStatus, lastId, pageSize);
            return orders.Select(MapToShortOrderDto);
            //return orders.Where(o => o.Delivery.CourierId == courierId).Select(MapToShortOrderDto);
        }


        // Методы для менеджера
        public async Task<IEnumerable<ShortOrderDto>> GetAllActiveOrdersAsync(int? lastId = null, int? pageSize=10)
        {

            var orders = await _uow.Orders.GetActiveOrdersAsync(lastId, pageSize);
            return orders.Select(o => MapToShortOrderDto(o));
        }

        public async Task<ShortOrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(id);
            return MapToShortOrderDto(order);
        }


        //public async Task<OrderDto> SubmitOrderAsync(int orderId)
        //{


        //    try
        //    {
        //        //await _uow.BeginTransactionAsync();

        //        var order = await _uow.Orders.GetOrderByIdAsync(orderId);
        //        order.DelStatusId = (int)OrderStatusEnum.IsBeingPrepared;

        //        await _uow.Save();
        //        //await _uow.CommitTransactionAsync();
        //        return MapToOrderDto(order);
        //    }
        //    catch
        //    {
        //        //await _uow.RollbackTransactionAsync();
        //        throw;
        //    }



        //}

     

        //private async Task ProcessOrderSubmission(Order order, string address)
        //{
        //    order.Address = address;
        //    order.OrderTime = DateTime.UtcNow;
        //    order.DelStatusId = (int)OrderStatusEnum.IsBeingFormed;

        //    await _uow.Orders.UpdateOrderAsync(order);
        //}

        // Отмена заказа
        public async Task<ShortOrderDto> CancelOrderAsync(int orderId)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Заказ", orderId);
            }
            if (!IsCancellable(order))
            {
                throw new InvalidOperationException("Заказ нельзя отменить в его текущем состоянии");
            }

            order.DelStatusId = (int)OrderStatusEnum.IsCancelled;
            order.CancellationTime = DateTime.UtcNow;
            try
            {
                await _uow.Orders.UpdateOrderAsync(order);
            }
            catch(DbUpdateException ex)
            {
                throw new MyDbException("Ошибка при отмене заказа. попробуйте позже", ex);
            }
           
            order = await _uow.Orders.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Заказ", orderId);
            }
            return MapToShortOrderDto(order);
        }

        private bool IsCancellable(Order order)
        {
            return order.DelStatusId == (int)OrderStatusEnum.IsBeingTransferred ||
                   order.DelStatusId == (int)OrderStatusEnum.IsBeingFormed ||
                   order.DelStatusId == (int)OrderStatusEnum.IsBeingPrepared;
        }
    }
}
