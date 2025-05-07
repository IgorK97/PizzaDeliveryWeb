using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class ShortOrderDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }

        public string ClientId { get; set; }
        public string? CourierId { get; set; }

        public OrderStatusEnum StatusId { get; set; }
        public string Status => StatusId switch
        {
            OrderStatusEnum.NotPlaced => "Не оформлен",
            OrderStatusEnum.IsBeingFormed => "Формируется",
            OrderStatusEnum.IsBeingPrepared => "Готовится",
            OrderStatusEnum.IsBeingTransferred => "Передается в доставку",
            OrderStatusEnum.HasBeenTransferred => "Передан курьеру",
            OrderStatusEnum.IsCancelled => "Отменен",
            OrderStatusEnum.IsDelivered => "Доставлен",
            OrderStatusEnum.IsNotDelivered => "Не доставлен",
            _ => "Уточняется",
        };
        public DateTime OrderTime { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime? DeliveryStartTime { get; set; }
        public DateTime? EndCookingTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? CancellationTime { get; set; }
        public string Address { get; set; }
        public List<OrderLineShortDto> OrderLines { get; set; } = new();
        public bool IsCancelled { get; set; }
        public bool IsDelivered { get; set; }
    }
}
