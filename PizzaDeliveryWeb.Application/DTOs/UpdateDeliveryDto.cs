using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class UpdateDeliveryDto
    {
        public int Id { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public bool? IsSuccessful { get; set; }
        public string? Comment { get; set; }
    }
}
