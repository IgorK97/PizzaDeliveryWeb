using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    
    public class OrderService
    {
        //private readonly IPizzaRepository _pizzaRepository;
        //private readonly IIngredientRepository _ingrRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderLineRepository _orderLineRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IPizzaSizeRepository _pizzaSizeRepository;
        
        public OrderService(IOrderRepository orderRepository, IOrderLineRepository orderLineRepository,
            IStatusRepository statusRepository,
            IPizzaSizeRepository pizzaSizeRepository)
        {
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _statusRepository = statusRepository;
            _pizzaSizeRepository = pizzaSizeRepository;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders.Select(o =>
            {
                List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                foreach (OrderLine ol in o.OrderLines)
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
                return new OrderDto
                {
                    Id = o.Id,
                    AcceptedTime = o.AcceptedTime,
                    ClientName = o.Client.FirstName + o.Client.LastName + 
                    (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
                    CompletionTime = o.CompletionTime,
                    FinalPrice = o.Price,
                    Weight = o.Weight,
                    OrderTime = o.OrderTime,
                    UserId = o.ClientId,
                    Status = o.DelStatus.Description,
                    OrderLines = oLinesDto
                };
            });
        }
        public async Task<IEnumerable<OrderDto>> GetClientOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var historyOrders = orders.Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced).ToList()
                .Select(o =>
                {
                    List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                    foreach (OrderLine ol in o.OrderLines)
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
                    return new OrderDto
                    {
                        Id = o.Id,
                        AcceptedTime = o.AcceptedTime,
                        ClientName = o.Client.FirstName + o.Client.LastName +
                        (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
                        CompletionTime = o.CompletionTime,
                        FinalPrice = o.Price,
                        Weight = o.Weight,
                        OrderTime = o.OrderTime,
                        UserId = o.ClientId,
                        Status = o.DelStatus.Description,
                        OrderLines = oLinesDto
                    };
                });
            return historyOrders;
        }
        public async Task<IEnumerable<OrderDto>> GetCourierOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersAsync();
            var historyOrders = orders.Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced
            && o.DelStatusId != (int)OrderStatusEnum.IsBeingFormed).ToList()
                .Select(o =>
                {
                    List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                    foreach (OrderLine ol in o.OrderLines)
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
                    return new OrderDto
                    {
                        Id = o.Id,
                        AcceptedTime = o.AcceptedTime,
                        ClientName = o.Client.FirstName + o.Client.LastName +
                        (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
                        CompletionTime = o.CompletionTime,
                        FinalPrice = o.Price,
                        Weight = o.Weight,
                        OrderTime = o.OrderTime,
                        UserId = o.ClientId,
                        Status = o.DelStatus.Description,
                        OrderLines = oLinesDto
                    };
                });
            var ownOrders = orders.Where(o => o.Deliveries.Any(d => d.CourierId == userId))
                .Select(o =>
                {
                    List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                    foreach (OrderLine ol in o.OrderLines)
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
                    return new OrderDto
                    {
                        Id = o.Id,
                        AcceptedTime = o.AcceptedTime,
                        ClientName = o.Client.FirstName + o.Client.LastName +
                        (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
                        CompletionTime = o.CompletionTime,
                        FinalPrice = o.Price,
                        Weight = o.Weight,
                        OrderTime = o.OrderTime,
                        UserId = o.ClientId,
                        Status = o.DelStatus.Description,
                        OrderLines = oLinesDto
                    };
                });
            var combineOrders = ownOrders.Concat(historyOrders).ToList();
            return combineOrders;
        }
        public async Task<IEnumerable<OrderDto>> GetAllActiveOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            var historyOrders = orders.Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced).ToList()
                .Select(o =>
                {
                    List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
                    foreach (OrderLine ol in o.OrderLines)
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
                    return new OrderDto
                    {
                        Id = o.Id,
                        AcceptedTime = o.AcceptedTime,
                        ClientName = o.Client.FirstName + o.Client.LastName +
                        (String.IsNullOrEmpty(o.Client.Surname) ? o.Client.Surname : ""),
                        CompletionTime = o.CompletionTime,
                        FinalPrice = o.Price,
                        Weight = o.Weight,
                        OrderTime = o.OrderTime,
                        UserId = o.ClientId,
                        Status = o.DelStatus.Description,
                        OrderLines = oLinesDto
                    };
                });
            return historyOrders;
        }
        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Заказ не найден");

            bool needsUpdate = false;
            //decimal totalPrice = 0;
            //decimal totalWeight = 0;
            OrderDto newOrderDto = new OrderDto
            {
                Id = order.Id,
                AcceptedTime = order.AcceptedTime,
                ClientName = order.Client.FirstName + order.Client.LastName + (String.IsNullOrEmpty(order.Client.Surname) ? order.Client.Surname : ""),
                CompletionTime = order.CompletionTime,
                FinalPrice = order.Price,
                Weight = order.Weight,
                OrderTime = order.OrderTime,
                UserId = order.ClientId,
                Status = order.DelStatus.Description,
                //OrderLines = oLinesDto

            };
            needsUpdate = await RecalculateOrder(order, newOrderDto);
            //List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
            //List<PizzaSize> pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();
            //foreach (OrderLine ol in order.OrderLines)
            //{
            //    var ps = pizzaSizes.FirstOrDefault(ps => ps.Id == ol.PizzaSizeId);

            //    decimal basePrice = ps.Price;
            //    decimal baseWeight = ps.Weight;

            //    decimal ingredientsPrice, ingredientsWeight;
            //    decimal additionalPrice, additionalWeight;

            //    decimal elementPrice = 0, elementWeight = 0;
            //    List<IngredientDto> ingrs = new List<IngredientDto>();
            //    foreach (Ingredient i in ol.Pizza.Ingredients)
            //    {
            //        ingrs.Add(new IngredientDto
            //        {
            //            Id = i.Id,
            //            Big = i.Big,
            //            Description = i.Description,
            //            Medium = i.Medium,
            //            Name = i.Name,
            //            PricePerGram = i.PricePerGram,
            //            Small = i.Small
            //        });
            //    }
            //    OrderLineDto newOrderLine = new OrderLineDto
            //    {
            //        Id = ol.Id,
            //        Price = ol.Price,
            //        Quantity = ol.Quantity,
            //        Weight = ol.Weight,
            //        Size = ol.PizzaSize.Name,
            //        Pizza = new PizzaDto
            //        {
            //            Id = ol.PizzaId,
            //            Description = ol.Pizza.Description,
            //            Image = ol.Pizza.Image,
            //            IsAvailable = ol.Pizza.IsAvailable,
            //            Name = ol.Pizza.Name,
            //            Ingredients = ingrs
            //        }
            //    };
            //    if (ps.Id == (int)PizzaSizeEnum.Small)
            //    {
            //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Small);
            //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Small);

            //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Small);
            //        additionalWeight = ol.Ingredients.Sum(i => i.Small);

            //    }
            //    else if (ps.Id == (int)PizzaSizeEnum.Medium)
            //    {
            //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Medium);
            //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Medium);

            //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Medium);
            //        additionalWeight = ol.Ingredients.Sum(i => i.Medium);
            //    }
            //    else
            //    {
            //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Big);
            //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Big);

            //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Big);
            //        additionalWeight = ol.Ingredients.Sum(i => i.Big);

            //    }
            //    elementPrice = basePrice + ingredientsPrice + additionalPrice;
            //    elementWeight = baseWeight + ingredientsWeight + additionalWeight;
            //    if (ol.Price != elementPrice || elementWeight != elementWeight)
            //    {
            //        needsUpdate = true;
            //        newOrderLine.Price = elementPrice;
            //        newOrderLine.Weight = elementWeight;
            //        ol.Price = elementPrice;
            //        ol.Weight = elementWeight;
            //    }
            //    oLinesDto.Add(newOrderLine);
            //    totalPrice += elementPrice;
            //    totalWeight += elementWeight;
            //}
            
            ;

            if (needsUpdate)
            {
                _orderRepository.UpdateOrderAsync(order);
                foreach(OrderLine ol in order.OrderLines)
                {
                    await _orderLineRepository.UpdateOrderLineAsync(ol);
                }
            }
            return newOrderDto;
        }

        private async Task<bool> RecalculateOrder(Order order, OrderDto newOrderDto)
        {
            decimal totalPrice = 0;
            decimal totalWeight = 0;
            bool needsUpdate = false;
            List<OrderLineDto> oLinesDto = new List<OrderLineDto>();
            List<PizzaSize> pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();
            foreach (OrderLine ol in order.OrderLines)
            {
                var ps = pizzaSizes.FirstOrDefault(ps => ps.Id == ol.PizzaSizeId);

                decimal basePrice = ps.Price;
                decimal baseWeight = ps.Weight;

                decimal ingredientsPrice, ingredientsWeight;
                decimal additionalPrice, additionalWeight;

                decimal elementPrice = 0, elementWeight = 0;
                List<IngredientDto> ingrs = new List<IngredientDto>();
                foreach (Ingredient i in ol.Pizza.Ingredients)
                {
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
                }
                OrderLineDto newOrderLine = new OrderLineDto
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
                };
                if (ps.Id == (int)PizzaSizeEnum.Small)
                {
                    ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Small);
                    ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Small);

                    additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Small);
                    additionalWeight = ol.Ingredients.Sum(i => i.Small);

                }
                else if (ps.Id == (int)PizzaSizeEnum.Medium)
                {
                    ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Medium);
                    ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Medium);

                    additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Medium);
                    additionalWeight = ol.Ingredients.Sum(i => i.Medium);
                }
                else
                {
                    ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Big);
                    ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Big);

                    additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Big);
                    additionalWeight = ol.Ingredients.Sum(i => i.Big);

                }
                elementPrice = basePrice + ingredientsPrice + additionalPrice;
                elementWeight = baseWeight + ingredientsWeight + additionalWeight;
                if (ol.Price != elementPrice || ol.Weight != elementWeight)
                {
                    needsUpdate = true;
                    newOrderLine.Price = elementPrice;
                    newOrderLine.Weight = elementWeight;
                    ol.Price = elementPrice;
                    ol.Weight = elementWeight;
                }
                oLinesDto.Add(newOrderLine);
                totalPrice += elementPrice*ol.Quantity;
                totalWeight += elementWeight*ol.Quantity;
            }

            if (order.Price != totalPrice || order.Weight != totalWeight)
            {
                needsUpdate = true;
                order.Price = totalPrice;
                newOrderDto.FinalPrice = totalPrice;
                order.Weight = totalWeight;
                newOrderDto.Weight = totalWeight;
            }

            return needsUpdate;

            //decimal totalPrice = 0, totalWeight = 0;
            //foreach(OrderLine ol in order.OrderLines)
            //{
            //    var ps = pizzaSizes.FirstOrDefault(ps => ps.Id == ol.PizzaSizeId);

            //    decimal basePrice = ps.Price;
            //    decimal baseWeight = ps.Weight;

            //    decimal ingredientsPrice, ingredientsWeight;
            //    decimal additionalPrice, additionalWeight;

            //    if (ps.Id == (int)PizzaSizeEnum.Small)
            //    {
            //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Small);
            //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Small);

            //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Small);
            //        additionalWeight = ol.Ingredients.Sum(i => i.Small);

            //    }
            //    else if (ps.Id == (int)PizzaSizeEnum.Medium)
            //    {
            //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Medium);
            //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Medium);

            //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Medium);
            //        additionalWeight = ol.Ingredients.Sum(i => i.Medium);
            //    }
            //    else
            //    {
            //        ingredientsPrice = ol.Pizza.Ingredients.Sum(i => i.PricePerGram * i.Big);
            //        ingredientsWeight = ol.Pizza.Ingredients.Sum(i => i.Big);

            //        additionalPrice = ol.Ingredients.Sum(i => i.PricePerGram * i.Big);
            //        additionalWeight = ol.Ingredients.Sum(i => i.Big);

            //    }
            //    totalPrice += basePrice + ingredientsPrice + additionalPrice;
            //    totalWeight += baseWeight + ingredientsWeight + additionalWeight;

            //}

        }

        public async Task<OrderDto> SubmitOrderAsync(ShortOrderDto orderDto)
        {
            
                var order = await _orderRepository.GetOrderByIdAsync(orderDto.Id);
                if (order == null)
                    throw new KeyNotFoundException("Заказ не найден");
            if (order.OrderTime != null)
                throw new InvalidOperationException("Заказ уже оформлен");
            order.Address = orderDto.Address;
            order.OrderTime = DateTime.UtcNow;
            var confirmedStatus = await _statusRepository.GetStatusByDescriptionAsync("Оформлен");
            if (confirmedStatus == null)
                throw new InvalidOperationException("Не удалось изменить статус заказа");
            order.DelStatusId = confirmedStatus.Id;

            await _orderRepository.UpdateOrderAsync(order);

            var newStatus = await _statusRepository.GetStatusByDescriptionAsync("Не оформлен");
            if (newStatus == null)
                throw new InvalidOperationException("Не удалось определить статус заказа");

            var newOrder = new Order
            {
                Id = 0,
                Address = order.Address,
                AcceptedTime = null,
                ClientId = order.ClientId,
                CompletionTime = null,
                DelStatusId = newStatus.Id,
                ManagerId = null,
                Price = 0,
                Weight = 0
            };
            await _orderRepository.AddOrderAsync(newOrder);
            OrderDto newOrderDto = new OrderDto
            {
                Id = newOrder.Id,
                AcceptedTime = null,
                CompletionTime = null,
                Address = newOrder.Address,
                ClientName = order.Client.FirstName + order.Client.LastName + (String.IsNullOrEmpty(order.Client.Surname != null ?
                order.Client.Surname : "")),
                FinalPrice = 0,
                Weight = 0,
                OrderTime = null,
                OrderLines = new List<OrderLineDto>(),
                UserId = newOrder.ClientId,
                Status = newStatus.Description
            };
            return newOrderDto;

        }

        public async Task<OrderDto> GetOrCreateCartAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var cart = orders.Where(o => o.DelStatusId == (int)OrderStatusEnum.NotPlaced).FirstOrDefault();
            if(cart == null)
            {
                cart = new Order
                {
                    Id = 0,
                    AcceptedTime = null,
                    Address = "",
                    CancellationTime = null,
                    ClientId = userId,
                    CompletionTime = null,
                    DelStatusId = (int)OrderStatusEnum.NotPlaced,
                    ManagerId = null,
                    OrderTime = null,
                    Price = 0,
                    Weight = 0,
                };
                _orderRepository.AddOrderAsync(cart);
                
            }
            return new OrderDto
            {
                Id = cart.Id,
                AcceptedTime = null,
                Address = "",
                CancellationTime = null,
                CompletionTime = null,
                Status = OrderStatusEnum.NotPlaced.ToString(),
                OrderTime = null,
                FinalPrice = 0,
                Weight = 0
            };
        }

        public async Task<OrderDto> CancelOrderAsync(int id)
        {
            Order order = await _orderRepository.GetOrderByIdAsync(id);
            if(order.DelStatusId== (int)OrderStatusEnum.IsBeingFormed ||
                order.DelStatusId == (int)OrderStatusEnum.IsBeingPrepared)
            {
                order.DelStatusId = (int)OrderStatusEnum.IsCanceled;
                await _orderRepository.UpdateOrderAsync(order);
                List<OrderLineDto> olines = new List<OrderLineDto>();
                foreach(OrderLine ol in order.OrderLines)
                {
                    List<IngredientDto> ingrs = new List<IngredientDto>();
                    List<IngredientDto> pizzaIngrs = new List<IngredientDto>();
                    foreach (Ingredient i in ol.Ingredients)
                        ingrs.Add(new IngredientDto
                        {
                            Id = i.Id,
                            Big = i.Big,
                            Medium = i.Medium,
                            Small = i.Small,
                            PricePerGram = i.PricePerGram,
                            Description = i.Description,
                            Name = i.Name
                        });
                    foreach(Ingredient i in ol.Pizza.Ingredients)
                        pizzaIngrs.Add(new IngredientDto
                        {
                            Id = i.Id,
                            Big = i.Big,
                            Medium = i.Medium,
                            Small = i.Small,
                            PricePerGram = i.PricePerGram,
                            Description = i.Description,
                            Name = i.Name
                        });
                    olines.Add(new OrderLineDto
                    {
                        Id = ol.Id,
                        OrderId = ol.OrderId,
                        Price = ol.Price,
                        Quantity = ol.Quantity,
                        Size = ol.PizzaSize.Name,
                        Weight = ol.Weight,
                        Ingredients=ingrs,
                        Pizza = new PizzaDto
                        {
                            Id = ol.PizzaId,
                            Image = ol.Pizza.Image,
                            Description = ol.Pizza.Description,
                            IsAvailable = ol.Pizza.IsAvailable,
                            Name = ol.Pizza.Name,
                            Ingredients = pizzaIngrs
                        }
                    });
                }
                OrderDto updatedOrderDto = new OrderDto
                {
                    Id = order.Id,
                    AcceptedTime = order.AcceptedTime,
                    Address = order.Address,
                    CancellationTime = order.CancellationTime,
                    ClientName = order.Client.FirstName + order.Client.LastName +
                    (String.IsNullOrEmpty(order.Client.Surname) ? "": order.Client.Surname),
                    CompletionTime=order.CompletionTime,
                    FinalPrice=order.Price,
                    Weight=order.Weight,
                    OrderTime=order.OrderTime,
                    UserId=order.ClientId,
                    Status=order.DelStatus.Description,
                    OrderLines=olines
                };
                return updatedOrderDto;
            }
            else
            {
                throw new Exception("Невозможно отменить заказ на данной стадии");
            }
        }

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
