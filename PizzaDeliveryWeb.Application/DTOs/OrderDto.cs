using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class OrderDto
    {
        
        public int Id { get; set; }
        public string ClientName { get; set; }
        
        public decimal FinalPrice { get; set; }
        
        public decimal Weight { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime? OrderTime { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? CancellationTime { get; set; }
        public string Address { get; set; }

        public virtual ICollection<OrderLineDto> OrderLines { get; set; } = new List<OrderLineDto>();
    }
}
