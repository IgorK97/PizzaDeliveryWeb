using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class ClientOrderDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime? DeliveryStartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? CancellationTime { get; set; }
        public string Address { get; set; }
        public List<OrderLineShortDto> OrderLines { get; set; } = new();
        public bool IsCancelled { get; set; }
        public bool IsDelivered { get; set; }
    }
}
