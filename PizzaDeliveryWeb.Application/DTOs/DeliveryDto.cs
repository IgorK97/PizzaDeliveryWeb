using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class DeliveryDto
    {
        public int Id { get; set; }
        public string CourierName { get; set; }
        public DateTime? AcceptanceTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public bool? IsSuccessful { get; set; }
        public string? Comment { get; set; }
        public virtual OrderDto Order { get; set; }
    }
}
