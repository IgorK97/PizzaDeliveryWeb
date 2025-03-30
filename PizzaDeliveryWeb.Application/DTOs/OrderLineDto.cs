using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class OrderLineDto
    {
        
        public int Id { get; set; }
        
        public int OrderId;

        public string Size { get; set; }
        public decimal Price { get; set; }
        
        public decimal Weight { get; set; }
        
        public int Quantity { get; set; }
        public virtual PizzaDto Pizza { get; set; }
        public virtual List<IngredientDto> Ingredients { get; set; }
    }
}
