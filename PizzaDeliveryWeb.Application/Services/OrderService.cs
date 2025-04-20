using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Application.DTOs;
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

        // В OrderService
        public async Task CompleteDeliveryAsync(int orderId, string courierId)
        {
            var order = await _uow.Orders.GetOrderWithDeliveryAsync(orderId);

            // Проверяем существование хотя бы одной доставки
            //if (!order.Deliveries.Any())
            //    throw new InvalidOperationException("Для заказа не созданы доставки");

            //// Берем последнюю доставку (предполагаем что статусы меняются в хронологическом порядке)
            //var lastDelivery = order.Deliveries
            //    .OrderByDescending(d => d.AcceptanceTime)
            //    .FirstOrDefault();

            // Проверяем назначение на курьера
            //if (lastDelivery.CourierId != courierId)
            //    throw new SecurityException("Текущий курьер не назначен на эту доставку");

            //if (lastDelivery.IsSuccessful != null)
            //    throw new InvalidOperationException("Доставка не в активном статуе");

            //// Обновление статусов
            //order.DelStatusId = (int)OrderStatusEnum.IsDelivered;
            //lastDelivery.IsSuccessful = true;
            //lastDelivery.DeliveryTime = DateTime.UtcNow;
            //order.CompletionTime = lastDelivery.DeliveryTime;

            await _uow.Save();
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(
        OrderStatusEnum status,
        int? lastId,
        int pageSize)
        {
            var orders = await _uow.Orders.GetOrdersByStatusAsync((int)status, lastId, pageSize);
            return orders.Select(MapToOrderDto);
        }

        public async Task StartDeliveryAsync(int orderId)
        {
            var order = await _uow.Orders.GetOrderWithDeliveryAsync(orderId);

            if (order.DelStatusId != (int)OrderStatusEnum.IsBeingPrepared)
                throw new InvalidOperationException("Невозможно начать доставку");

            order.DelStatusId = (int)OrderStatusEnum.IsBeingTransferred;

            // Создаем запись доставки
            var delivery = new Delivery
            {
                OrderId = orderId,
                AcceptanceTime = DateTime.UtcNow
            };

            await _uow.Deliveries.AddDeliveryAsync(delivery);
            await _uow.Save();
        }

        private async Task<Order> ValidateOrderForAcceptance(int orderId)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new ArgumentException("Заказ не найден");

            if (order.DelStatusId != (int)OrderStatusEnum.NotPlaced)
                throw new InvalidOperationException("Невозможно принять заказ в текущем статусе");

            return order;
        }

        public async Task AcceptOrderAsync(int orderId, string managerId)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var order = await ValidateOrderForAcceptance(orderId);
                order.ManagerId = managerId;
                order.DelStatusId = (int)OrderStatusEnum.IsBeingPrepared;

                await _uow.Save();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
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
        private ClientOrderDto MapToClientOrderDto(Order order)
        {
            return new ClientOrderDto
            {
                Id = order.Id,
                Price = order.Price,
                Status = order.DelStatus.Description,
                OrderTime = (DateTime)order.OrderTime,
                Address = order.Address,
                AcceptedTime = order.AcceptedTime,
                CancellationTime = order.CancellationTime,
                CompletionTime = order.CompletionTime,
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
                   Quantity = ol.Quantity,
                   Price = ol.Price,
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

        private async Task<IEnumerable<OrderDto>> GetOrdersByFilterAsync(
        Func<Order, bool> filter,
        int? lastId = null,
        int pageSize = 10)
        {
            var orders = await _uow.Orders.GetOrdersAsync(lastId, pageSize);
            return orders.Where(filter).Select(MapToOrderDto);
        }

        

        

        public async Task<IEnumerable<ClientOrderDto>> GetClientOrderHistoryAsync(string userId, int? lastId=null, int pageSize=10)
        {
            var orders = await _uow.Orders.GetOrdersByClientIdAsync(userId, lastId, pageSize);
            return orders.Select(MapToClientOrderDto);

            //return await GetOrdersByFilterAsync(
            //    o => o.ClientId == userId &&
            //         o.DelStatusId != (int)OrderStatusEnum.NotPlaced);
        }

        // Методы для курьера
        //public async Task<IEnumerable<OrderDto>> GetCourierActiveOrdersAsync(string courierId)
        //{

        //    //return await GetOrdersByFilterAsync(
        //    //    o => o.Deliveries.Any(d => d.CourierId == courierId) &&
        //    //         o.DelStatusId == (int)OrderStatusEnum.IsBeingTransferred);
        //}

        public async Task<IEnumerable<OrderDto>> GetCourierActiveOrdersAsync(string courierId)
        {
            var orders =  await _uow.Orders.GetOrdersAsync();
            return orders.Where(o => o.Delivery.CourierId == courierId).Select(MapToOrderDto);
        }


        // Методы для менеджера
        public async Task<IEnumerable<OrderDto>> GetAllActiveOrdersAsync()
        {
            return await GetOrdersByFilterAsync(
                o => o.DelStatusId != (int)OrderStatusEnum.IsDelivered &&
                     o.DelStatusId != (int)OrderStatusEnum.IsCancelled);
        }

        public async Task<ClientOrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(id);
            return MapToClientOrderDto(order);
        }


        public async Task<OrderDto> SubmitOrderAsync(int orderId)
        {


            try
            {
                await _uow.BeginTransactionAsync();

                var order = await _uow.Orders.GetOrderByIdAsync(orderId);
                order.DelStatusId = (int)OrderStatusEnum.IsBeingPrepared;

                await _uow.Save();
                await _uow.CommitTransactionAsync();
                return MapToOrderDto(order);
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }



            //using var transaction = await _context.Database.BeginTransactionAsync();
            //try
            //{
            //    var order = await ValidateAndLockOrder(orderId);

            //    await ProcessOrderSubmission(order, address);
            //    var newCart = await CreateNewCart(order.ClientId);

            //    await transaction.CommitAsync();

            //    return MapToOrderDto(newCart);
            //}
            //catch
            //{
            //    await transaction.RollbackAsync();
            //    throw;
            //}
        }

        private async Task<Order> ValidateAndLockOrder(int orderId)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(orderId);
            if (order?.DelStatusId != (int)OrderStatusEnum.NotPlaced)
            {
                throw new InvalidOperationException("Невозможно оформить данный заказ");
            }
            return order;
        }

        private async Task ProcessOrderSubmission(Order order, string address)
        {
            order.Address = address;
            order.OrderTime = DateTime.UtcNow;
            order.DelStatusId = (int)OrderStatusEnum.IsBeingFormed;

            await _uow.Orders.UpdateOrderAsync(order);
        }

        // Отмена заказа
        public async Task<ClientOrderDto> CancelOrderAsync(int orderId)
        {
            var order = await _uow.Orders.GetOrderByIdAsync(orderId);
            if (!IsCancellable(order))
            {
                throw new InvalidOperationException("Заказ нельзя отменить");
            }

            order.DelStatusId = (int)OrderStatusEnum.IsCancelled;
            await _uow.Orders.UpdateOrderAsync(order);
            return MapToClientOrderDto(order);
        }

        private bool IsCancellable(Order order)
        {
            return order.DelStatusId == (int)OrderStatusEnum.IsBeingTransferred ||
                   order.DelStatusId == (int)OrderStatusEnum.IsBeingFormed ||
                   order.DelStatusId == (int)OrderStatusEnum.IsBeingPrepared;
        }

        //public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
        //{
        //    var orders = await _orderRepository.GetOrdersAsync();
        //    return orders.Select(o =>
        //    {
        //        List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //        foreach (OrderLine ol in o.OrderLines)
        //        {
        //            List<IngredientDto> ingrs = new List<IngredientDto>();
        //            foreach (Ingredient i in ol.Pizza.Ingredients)
        //                ingrs.Add(new IngredientDto
        //                {
        //                    Id = i.Id,
        //                    Big = i.Big,
        //                    Description = i.Description,
        //                    Medium = i.Medium,
        //                    Name = i.Name,
        //                    PricePerGram = i.PricePerGram,
        //                    Small = i.Small
        //                });
        //            oLinesDto.Add(new OrderLineDto
        //            {
        //                Id = ol.Id,
        //                Price = ol.Price,
        //                Quantity = ol.Quantity,
        //                Weight = ol.Weight,
        //                Size = ol.PizzaSize.Name,
        //                Pizza = new PizzaDto
        //                {
        //                    Id = ol.PizzaId,
        //                    Description = ol.Pizza.Description,
        //                    Image = ol.Pizza.Image,
        //                    IsAvailable = ol.Pizza.IsAvailable,
        //                    Name = ol.Pizza.Name,
        //                    Ingredients = ingrs
        //                }
        //            });
        //        }
        //        return new OrderDto
        //        {
        //            Id = o.Id,
        //            AcceptedTime = o.AcceptedTime,
        //            ClientName = o.Client.FirstName + o.Client.LastName + 
        //            (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
        //            CompletionTime = o.CompletionTime,
        //            FinalPrice = o.Price,
        //            Weight = o.Weight,
        //            OrderTime = o.OrderTime,
        //            UserId = o.ClientId,
        //            Status = o.DelStatus.Description,
        //            OrderLines = oLinesDto
        //        };
        //    });
        //}
        //public async Task<IEnumerable<OrderDto>> GetClientOrdersAsync(string userId)
        //{
        //    var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        //    var historyOrders = orders.Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced).ToList()
        //        .Select(o =>
        //        {
        //            List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //            foreach (OrderLine ol in o.OrderLines)
        //            {
        //                List<IngredientDto> ingrs = new List<IngredientDto>();
        //                foreach (Ingredient i in ol.Pizza.Ingredients)
        //                    ingrs.Add(new IngredientDto
        //                    {
        //                        Id = i.Id,
        //                        Big = i.Big,
        //                        Description = i.Description,
        //                        Medium = i.Medium,
        //                        Name = i.Name,
        //                        PricePerGram = i.PricePerGram,
        //                        Small = i.Small
        //                    });
        //                oLinesDto.Add(new OrderLineDto
        //                {
        //                    Id = ol.Id,
        //                    Price = ol.Price,
        //                    Quantity = ol.Quantity,
        //                    Weight = ol.Weight,
        //                    Size = ol.PizzaSize.Name,
        //                    Pizza = new PizzaDto
        //                    {
        //                        Id = ol.PizzaId,
        //                        Description = ol.Pizza.Description,
        //                        Image = ol.Pizza.Image,
        //                        IsAvailable = ol.Pizza.IsAvailable,
        //                        Name = ol.Pizza.Name,
        //                        Ingredients = ingrs
        //                    }
        //                });
        //            }
        //            return new OrderDto
        //            {
        //                Id = o.Id,
        //                AcceptedTime = o.AcceptedTime,
        //                ClientName = o.Client.FirstName + o.Client.LastName +
        //                (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
        //                CompletionTime = o.CompletionTime,
        //                FinalPrice = o.Price,
        //                Weight = o.Weight,
        //                OrderTime = o.OrderTime,
        //                UserId = o.ClientId,
        //                Status = o.DelStatus.Description,
        //                OrderLines = oLinesDto
        //            };
        //        });
        //    return historyOrders;
        //}
        //public async Task<IEnumerable<OrderDto>> GetCourierOrdersAsync(string userId)
        //{
        //    var orders = await _orderRepository.GetOrdersAsync();
        //    var historyOrders = orders.Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced
        //    && o.DelStatusId != (int)OrderStatusEnum.IsBeingFormed).ToList()
        //        .Select(o =>
        //        {
        //            List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //            foreach (OrderLine ol in o.OrderLines)
        //            {
        //                List<IngredientDto> ingrs = new List<IngredientDto>();
        //                foreach (Ingredient i in ol.Pizza.Ingredients)
        //                    ingrs.Add(new IngredientDto
        //                    {
        //                        Id = i.Id,
        //                        Big = i.Big,
        //                        Description = i.Description,
        //                        Medium = i.Medium,
        //                        Name = i.Name,
        //                        PricePerGram = i.PricePerGram,
        //                        Small = i.Small
        //                    });
        //                oLinesDto.Add(new OrderLineDto
        //                {
        //                    Id = ol.Id,
        //                    Price = ol.Price,
        //                    Quantity = ol.Quantity,
        //                    Weight = ol.Weight,
        //                    Size = ol.PizzaSize.Name,
        //                    Pizza = new PizzaDto
        //                    {
        //                        Id = ol.PizzaId,
        //                        Description = ol.Pizza.Description,
        //                        Image = ol.Pizza.Image,
        //                        IsAvailable = ol.Pizza.IsAvailable,
        //                        Name = ol.Pizza.Name,
        //                        Ingredients = ingrs
        //                    }
        //                });
        //            }
        //            return new OrderDto
        //            {
        //                Id = o.Id,
        //                AcceptedTime = o.AcceptedTime,
        //                ClientName = o.Client.FirstName + o.Client.LastName +
        //                (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
        //                CompletionTime = o.CompletionTime,
        //                FinalPrice = o.Price,
        //                Weight = o.Weight,
        //                OrderTime = o.OrderTime,
        //                UserId = o.ClientId,
        //                Status = o.DelStatus.Description,
        //                OrderLines = oLinesDto
        //            };
        //        });
        //    var ownOrders = orders.Where(o => o.Deliveries.Any(d => d.CourierId == userId))
        //        .Select(o =>
        //        {
        //            List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //            foreach (OrderLine ol in o.OrderLines)
        //            {
        //                List<IngredientDto> ingrs = new List<IngredientDto>();
        //                foreach (Ingredient i in ol.Pizza.Ingredients)
        //                    ingrs.Add(new IngredientDto
        //                    {
        //                        Id = i.Id,
        //                        Big = i.Big,
        //                        Description = i.Description,
        //                        Medium = i.Medium,
        //                        Name = i.Name,
        //                        PricePerGram = i.PricePerGram,
        //                        Small = i.Small
        //                    });
        //                oLinesDto.Add(new OrderLineDto
        //                {
        //                    Id = ol.Id,
        //                    Price = ol.Price,
        //                    Quantity = ol.Quantity,
        //                    Weight = ol.Weight,
        //                    Size = ol.PizzaSize.Name,
        //                    Pizza = new PizzaDto
        //                    {
        //                        Id = ol.PizzaId,
        //                        Description = ol.Pizza.Description,
        //                        Image = ol.Pizza.Image,
        //                        IsAvailable = ol.Pizza.IsAvailable,
        //                        Name = ol.Pizza.Name,
        //                        Ingredients = ingrs
        //                    }
        //                });
        //            }
        //            return new OrderDto
        //            {
        //                Id = o.Id,
        //                AcceptedTime = o.AcceptedTime,
        //                ClientName = o.Client.FirstName + o.Client.LastName +
        //                (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
        //                CompletionTime = o.CompletionTime,
        //                FinalPrice = o.Price,
        //                Weight = o.Weight,
        //                OrderTime = o.OrderTime,
        //                UserId = o.ClientId,
        //                Status = o.DelStatus.Description,
        //                OrderLines = oLinesDto
        //            };
        //        });
        //    var combineOrders = ownOrders.Concat(historyOrders).ToList();
        //    return combineOrders;
        //}
        //public async Task<IEnumerable<OrderDto>> GetAllActiveOrdersAsync()
        //{
        //    var orders = await _orderRepository.GetOrdersAsync();
        //    var historyOrders = orders.Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced).ToList()
        //        .Select(o =>
        //        {
        //            List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //            foreach (OrderLine ol in o.OrderLines)
        //            {
        //                List<IngredientDto> ingrs = new List<IngredientDto>();
        //                foreach (Ingredient i in ol.Pizza.Ingredients)
        //                    ingrs.Add(new IngredientDto
        //                    {
        //                        Id = i.Id,
        //                        Big = i.Big,
        //                        Description = i.Description,
        //                        Medium = i.Medium,
        //                        Name = i.Name,
        //                        PricePerGram = i.PricePerGram,
        //                        Small = i.Small
        //                    });
        //                oLinesDto.Add(new OrderLineDto
        //                {
        //                    Id = ol.Id,
        //                    Price = ol.Price,
        //                    Quantity = ol.Quantity,
        //                    Weight = ol.Weight,
        //                    Size = ol.PizzaSize.Name,
        //                    Pizza = new PizzaDto
        //                    {
        //                        Id = ol.PizzaId,
        //                        Description = ol.Pizza.Description,
        //                        Image = ol.Pizza.Image,
        //                        IsAvailable = ol.Pizza.IsAvailable,
        //                        Name = ol.Pizza.Name,
        //                        Ingredients = ingrs
        //                    }
        //                });
        //            }
        //            return new OrderDto
        //            {
        //                Id = o.Id,
        //                AcceptedTime = o.AcceptedTime,
        //                ClientName = o.Client.FirstName + o.Client.LastName +
        //                (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
        //                CompletionTime = o.CompletionTime,
        //                FinalPrice = o.Price,
        //                Weight = o.Weight,
        //                OrderTime = o.OrderTime,
        //                UserId = o.ClientId,
        //                Status = o.DelStatus.Description,
        //                OrderLines = oLinesDto
        //            };
        //        });
        //    return historyOrders;
        //}
        //public async Task<OrderDto> GetOrderByIdAsync(int id)
        //{
        //    var order = await _orderRepository.GetOrderByIdAsync(id);
        //    if (order == null)
        //        throw new KeyNotFoundException("Заказ не найден");

        //    bool needsUpdate = false;
        //    //decimal totalPrice = 0;
        //    //decimal totalWeight = 0;
        //    OrderDto newOrderDto = new OrderDto
        //    {
        //        Id = order.Id,
        //        AcceptedTime = order.AcceptedTime,
        //        ClientName = order.Client.FirstName + order.Client.LastName + (String.IsNullOrEmpty(order.Client.Surname) ? order.Client.Surname : ""),
        //        CompletionTime = order.CompletionTime,
        //        FinalPrice = order.Price,
        //        Weight = order.Weight,
        //        OrderTime = order.OrderTime,
        //        UserId = order.ClientId,
        //        Status = order.DelStatus.Description,
        //        //OrderLines = oLinesDto

        //    };
        //    needsUpdate = await RecalculateOrder(order, newOrderDto);
        //    //List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //    //List<PizzaSize> pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();
        //    //foreach (OrderLine ol in order.OrderLines)
        //    //{
        //    //    var ps = pizzaSizes.FirstOrDefault(ps => ps.Id == ol.PizzaSizeId);

        //    //    decimal basePrice = ps.Price;
        //    //    decimal baseWeight = ps.Weight;

        //    //    decimal ingredientsPrice, ingredientsWeight;
        //    //    decimal additionalPrice, additionalWeight;

        //    //    decimal elementPrice = 0, elementWeight = 0;
        //    //    List<IngredientDto> ingrs = new List<IngredientDto>();
        //    //    foreach (Ingredient i in ol.Pizza.Ingredients)
        //    //    {
        //    //        ingrs.Add(new IngredientDto
        //    //        {
        //    //            Id = i.Id,
        //    //            Big = i.Big,
        //    //            Description = i.Description,
        //    //            Medium = i.Medium,
        //    //            Name = i.Name,
        //    //            PricePerGram = i.PricePerGram,
        //    //            Small = i.Small
        //    //        });
        //    //    }
        //    //    OrderLineDto newOrderLine = new OrderLineDto
        //    //    {
        //    //        Id = ol.Id,
        //    //        Price = ol.Price,
        //    //        Quantity = ol.Quantity,
        //    //        Weight = ol.Weight,
        //    //        Size = ol.PizzaSize.Name,
        //    //        Pizza = new PizzaDto
        //    //        {
        //    //            Id = ol.PizzaId,
        //    //            Description = ol.Pizza.Description,
        //    //            Image = ol.Pizza.Image,
        //    //            IsAvailable = ol.Pizza.IsAvailable,
        //    //            Name = ol.Pizza.Name,
        //    //            Ingredients = ingrs
        //    //        }
        //    //    };
        //    //    if (ps.Id == (int)PizzaSizeEnum.Small)
        //    //    {
        //    //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Small);
        //    //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Small);

        //    //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Small);
        //    //        additionalWeight = ol.Ingredients.Sum(i => i.Small);

        //    //    }
        //    //    else if (ps.Id == (int)PizzaSizeEnum.Medium)
        //    //    {
        //    //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Medium);
        //    //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Medium);

        //    //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Medium);
        //    //        additionalWeight = ol.Ingredients.Sum(i => i.Medium);
        //    //    }
        //    //    else
        //    //    {
        //    //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Big);
        //    //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Big);

        //    //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Big);
        //    //        additionalWeight = ol.Ingredients.Sum(i => i.Big);

        //    //    }
        //    //    elementPrice = basePrice + ingredientsPrice + additionalPrice;
        //    //    elementWeight = baseWeight + ingredientsWeight + additionalWeight;
        //    //    if (ol.Price != elementPrice || elementWeight != elementWeight)
        //    //    {
        //    //        needsUpdate = true;
        //    //        newOrderLine.Price = elementPrice;
        //    //        newOrderLine.Weight = elementWeight;
        //    //        ol.Price = elementPrice;
        //    //        ol.Weight = elementWeight;
        //    //    }
        //    //    oLinesDto.Add(newOrderLine);
        //    //    totalPrice += elementPrice;
        //    //    totalWeight += elementWeight;
        //    //}
            
        //    ;

        //    if (needsUpdate)
        //    {
        //        _orderRepository.UpdateOrderAsync(order);
        //        foreach(OrderLine ol in order.OrderLines)
        //        {
        //            await _orderLineRepository.UpdateOrderLineAsync(ol);
        //        }
        //    }
        //    return newOrderDto;
        //}

        //private async Task<bool> RecalculateOrder(Order order, OrderDto newOrderDto)
        //{
        //    decimal totalPrice = 0;
        //    decimal totalWeight = 0;
        //    bool needsUpdate = false;
        //    List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
        //    List<Domain.Entities.PizzaSize> pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();
        //    foreach (OrderLine ol in order.OrderLines)
        //    {
        //        var ps = pizzaSizes.FirstOrDefault(ps => ps.Id == ol.PizzaSizeId);

        //        decimal basePrice = ps.Price;
        //        decimal baseWeight = ps.Weight;

        //        decimal ingredientsPrice, ingredientsWeight;
        //        decimal additionalPrice, additionalWeight;

        //        decimal elementPrice = 0, elementWeight = 0;
        //        List<IngredientDto> ingrs = new List<IngredientDto>();
        //        foreach (Ingredient i in ol.Pizza.Ingredients)
        //        {
        //            ingrs.Add(new IngredientDto
        //            {
        //                Id = i.Id,
        //                Big = i.Big,
        //                Description = i.Description,
        //                Medium = i.Medium,
        //                Name = i.Name,
        //                PricePerGram = i.PricePerGram,
        //                Small = i.Small
        //            });
        //        }
        //        OrderLineDto newOrderLine = new OrderLineDto
        //        {
        //            Id = ol.Id,
        //            Price = ol.Price,
        //            Quantity = ol.Quantity,
        //            Weight = ol.Weight,
        //            Size = ol.PizzaSize.Name,
        //            Pizza = new PizzaDto
        //            {
        //                Id = ol.PizzaId,
        //                Description = ol.Pizza.Description,
        //                Image = ol.Pizza.Image,
        //                IsAvailable = ol.Pizza.IsAvailable,
        //                Name = ol.Pizza.Name,
        //                Ingredients = ingrs
        //            }
        //        };
        //        if (ps.Id == (int)PizzaSizeEnum.Small)
        //        {
        //            ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Small);
        //            ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Small);

        //            additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Small);
        //            additionalWeight = ol.Ingredients.Sum(i => i.Small);

        //        }
        //        else if (ps.Id == (int)PizzaSizeEnum.Medium)
        //        {
        //            ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Medium);
        //            ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Medium);

        //            additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Medium);
        //            additionalWeight = ol.Ingredients.Sum(i => i.Medium);
        //        }
        //        else
        //        {
        //            ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Big);
        //            ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Big);

        //            additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Big);
        //            additionalWeight = ol.Ingredients.Sum(i => i.Big);

        //        }
        //        elementPrice = basePrice + ingredientsPrice + additionalPrice;
        //        elementWeight = baseWeight + ingredientsWeight + additionalWeight;
        //        if (ol.Price != elementPrice || ol.Weight != elementWeight)
        //        {
        //            needsUpdate = true;
        //            newOrderLine.Price = elementPrice;
        //            newOrderLine.Weight = elementWeight;
        //            ol.Price = elementPrice;
        //            ol.Weight = elementWeight;
        //        }
        //        oLinesDto.Add(newOrderLine);
        //        totalPrice += elementPrice*ol.Quantity;
        //        totalWeight += elementWeight*ol.Quantity;
        //    }

        //    if (order.Price != totalPrice || order.Weight != totalWeight)
        //    {
        //        needsUpdate = true;
        //        order.Price = totalPrice;
        //        newOrderDto.FinalPrice = totalPrice;
        //        order.Weight = totalWeight;
        //        newOrderDto.Weight = totalWeight;
        //    }

        //    return needsUpdate;

        //    //decimal totalPrice = 0, totalWeight = 0;
        //    //foreach(OrderLine ol in order.OrderLines)
        //    //{
        //    //    var ps = pizzaSizes.FirstOrDefault(ps => ps.Id == ol.PizzaSizeId);

        //    //    decimal basePrice = ps.Price;
        //    //    decimal baseWeight = ps.Weight;

        //    //    decimal ingredientsPrice, ingredientsWeight;
        //    //    decimal additionalPrice, additionalWeight;

        //    //    if (ps.Id == (int)PizzaSizeEnum.Small)
        //    //    {
        //    //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Small);
        //    //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Small);

        //    //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Small);
        //    //        additionalWeight = ol.Ingredients.Sum(i => i.Small);

        //    //    }
        //    //    else if (ps.Id == (int)PizzaSizeEnum.Medium)
        //    //    {
        //    //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Medium);
        //    //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Medium);

        //    //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Medium);
        //    //        additionalWeight = ol.Ingredients.Sum(i => i.Medium);
        //    //    }
        //    //    else
        //    //    {
        //    //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Big);
        //    //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Big);

        //    //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Big);
        //    //        additionalWeight = ol.Ingredients.Sum(i => i.Big);

        //    //    }
        //    //    totalPrice += basePrice + ingredientsPrice + additionalPrice;
        //    //    totalWeight += baseWeight + ingredientsWeight + additionalWeight;

        //    //}

        //}

        //public async Task<OrderDto> SubmitOrderAsync(ShortOrderDto orderDto)
        //{
            
        //        var order = await _orderRepository.GetOrderByIdAsync(orderDto.Id);
        //        if (order == null)
        //            throw new KeyNotFoundException("Заказ не найден");
        //    if (order.OrderTime != null)
        //        throw new InvalidOperationException("Заказ уже оформлен");
        //    order.Address = orderDto.Address;
        //    order.OrderTime = DateTime.UtcNow;
        //    var confirmedStatus = await _statusRepository.GetStatusByDescriptionAsync("Оформлен");
        //    if (confirmedStatus == null)
        //        throw new InvalidOperationException("Не удалось изменить статус заказа");
        //    order.DelStatusId = confirmedStatus.Id;

        //    await _orderRepository.UpdateOrderAsync(order);

        //    var newStatus = await _statusRepository.GetStatusByDescriptionAsync("Не оформлен");
        //    if (newStatus == null)
        //        throw new InvalidOperationException("Не удалось определить статус заказа");

        //    var newOrder = new Order
        //    {
        //        Id = 0,
        //        Address = order.Address,
        //        AcceptedTime = null,
        //        ClientId = order.ClientId,
        //        CompletionTime = null,
        //        DelStatusId = newStatus.Id,
        //        ManagerId = null,
        //        Price = 0,
        //        Weight = 0
        //    };
        //    await _orderRepository.AddOrderAsync(newOrder);
        //    OrderDto newOrderDto = new OrderDto
        //    {
        //        Id = newOrder.Id,
        //        AcceptedTime = null,
        //        CompletionTime = null,
        //        Address = newOrder.Address,
        //        ClientName = order.Client.FirstName + order.Client.LastName + (String.IsNullOrEmpty(order.Client.Surname != null ?
        //        order.Client.Surname : "")),
        //        FinalPrice = 0,
        //        Weight = 0,
        //        OrderTime = null,
        //        OrderLines = new List<OrderLineDto>(),
        //        UserId = newOrder.ClientId,
        //        Status = newStatus.Description
        //    };
        //    return newOrderDto;

        //}

        //public async Task<OrderDto> GetOrCreateCartAsync(string userId)
        //{
        //    var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        //    var cart = orders.Where(o => o.DelStatusId == (int)OrderStatusEnum.NotPlaced).FirstOrDefault();
        //    if(cart == null)
        //    {
        //        cart = new Order
        //        {
        //            Id = 0,
        //            AcceptedTime = null,
        //            Address = "",
        //            CancellationTime = null,
        //            ClientId = userId,
        //            CompletionTime = null,
        //            DelStatusId = (int)OrderStatusEnum.NotPlaced,
        //            ManagerId = null,
        //            OrderTime = null,
        //            Price = 0,
        //            Weight = 0,
        //        };
        //        _orderRepository.AddOrderAsync(cart);
                
        //    }
        //    return new OrderDto
        //    {
        //        Id = cart.Id,
        //        AcceptedTime = null,
        //        Address = "",
        //        CancellationTime = null,
        //        CompletionTime = null,
        //        Status = OrderStatusEnum.NotPlaced.ToString(),
        //        OrderTime = null,
        //        FinalPrice = 0,
        //        Weight = 0
        //    };
        //}

        //public async Task<OrderDto> CancelOrderAsync(int id)
        //{
        //    Order order = await _orderRepository.GetOrderByIdAsync(id);
        //    if(order.DelStatusId== (int)OrderStatusEnum.IsBeingFormed ||
        //        order.DelStatusId == (int)OrderStatusEnum.IsBeingPrepared)
        //    {
        //        order.DelStatusId = (int)OrderStatusEnum.IsCancelled;
        //        await _orderRepository.UpdateOrderAsync(order);
        //        List<OrderLineDto> olines = new List<OrderLineDto>();
        //        foreach(OrderLine ol in order.OrderLines)
        //        {
        //            List<IngredientDto> ingrs = new List<IngredientDto>();
        //            List<IngredientDto> pizzaIngrs = new List<IngredientDto>();
        //            foreach (Ingredient i in ol.Ingredients)
        //                ingrs.Add(new IngredientDto
        //                {
        //                    Id = i.Id,
        //                    Big = i.Big,
        //                    Medium = i.Medium,
        //                    Small = i.Small,
        //                    PricePerGram = i.PricePerGram,
        //                    Description = i.Description,
        //                    Name = i.Name
        //                });
        //            foreach(Ingredient i in ol.Pizza.Ingredients)
        //                pizzaIngrs.Add(new IngredientDto
        //                {
        //                    Id = i.Id,
        //                    Big = i.Big,
        //                    Medium = i.Medium,
        //                    Small = i.Small,
        //                    PricePerGram = i.PricePerGram,
        //                    Description = i.Description,
        //                    Name = i.Name
        //                });
        //            olines.Add(new OrderLineDto
        //            {
        //                Id = ol.Id,
        //                OrderId = ol.OrderId,
        //                Price = ol.Price,
        //                Quantity = ol.Quantity,
        //                Size = ol.PizzaSize.Name,
        //                Weight = ol.Weight,
        //                Ingredients=ingrs,
        //                Pizza = new PizzaDto
        //                {
        //                    Id = ol.PizzaId,
        //                    Image = ol.Pizza.Image,
        //                    Description = ol.Pizza.Description,
        //                    IsAvailable = ol.Pizza.IsAvailable,
        //                    Name = ol.Pizza.Name,
        //                    Ingredients = pizzaIngrs
        //                }
        //            });
        //        }
        //        OrderDto updatedOrderDto = new OrderDto
        //        {
        //            Id = order.Id,
        //            AcceptedTime = order.AcceptedTime,
        //            Address = order.Address,
        //            CancellationTime = order.CancellationTime,
        //            ClientName = order.Client.FirstName + order.Client.LastName +
        //            (String.IsNullOrEmpty(order.Client.Surname) ? "": order.Client.Surname),
        //            CompletionTime=order.CompletionTime,
        //            FinalPrice=order.Price,
        //            Weight=order.Weight,
        //            OrderTime=order.OrderTime,
        //            UserId=order.ClientId,
        //            Status=order.DelStatus.Description,
        //            OrderLines=olines
        //        };
        //        return updatedOrderDto;
        //    }
        //    else
        //    {
        //        throw new Exception("Невозможно отменить заказ на данной стадии");
        //    }
        //}

        //public async Task<OrderDto> UpdateOrderAsync(UpdateOrderDto orderDto)
        //{
        //    var order = await _orderRepository.GetOrderByIdAsync(orderDto.Id);
        //    if (order == null)
        //        throw new KeyNotFoundException("Заказ не найден");
            
        //    //var pizza = await _pizzaRepository.GetPizzaByIdAsync(pizzaDto.Id);
        //    //if (pizza != null)
        //    //{
        //    //    pizza.Name = pizzaDto.Name;
        //    //    pizza.Description = pizzaDto.Description;
        //    //    pizza.Image = pizzaDto.Image;
        //    //    pizza.IsAvailable = pizzaDto.IsAvailable;
        //    //    //ingredients
        //    //    await _pizzaRepository.UpdatePizzaAsync(pizza);
        //    //}
        //}

        //Удаление заказа не предусмотрено (но его можно будет отменить)
        //public async Task DeletePizzaAsync(int id)
        //{
        //    try
        //    {
        //        await _orderRepository.DeleteOrderAsync(id);
        //    }
        //    catch (InvalidOperationException ex)
        //    {

        //        throw new ApplicationException($"Error deleting order: {ex.Message}", ex);
        //    }
        //}



    }
}
