using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class CreateDeliveryDto
    {
        public int OrderId { get; set; }
        public string CourierId { get; set; }
    }
}
