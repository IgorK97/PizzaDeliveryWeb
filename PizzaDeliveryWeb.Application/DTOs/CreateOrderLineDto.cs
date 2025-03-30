using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class CreateOrderLineDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int PizzaId { get; set; }
        public int PizzaSizeId { get; set; }
        public int Quantity { get; set; }
        public bool Custom { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }

        public List<int> AddedIngredients { get; set; } = new List<int>();
    }
}
