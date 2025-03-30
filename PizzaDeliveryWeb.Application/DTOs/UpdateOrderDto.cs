using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class UpdateOrderDto
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Address { get; set; }

        public List<int> Ingredients { get; set; } = new List<int>();
        //public List<CreateOrderLineDto> OrderLines { get; set; }

    }
}
