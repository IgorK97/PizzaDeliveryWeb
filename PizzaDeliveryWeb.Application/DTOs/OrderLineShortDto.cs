using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class OrderLineShortDto
    {
        

        public int Id { get; set; }
        public int PizzaId { get; set; }
        public string PizzaImage { get; set; }
        public string PizzaName { get; set; }
        public PizzaSizeEnum PizzaSizeId { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }

        public decimal Weight { get; set; }

        public int Quantity { get; set; }
        public List<string> AddedIngredients { get; set; } = new List<string>();
        
    }
}
