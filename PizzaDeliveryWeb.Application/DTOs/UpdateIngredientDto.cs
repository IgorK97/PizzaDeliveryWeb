using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class UpdateIngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Small { get; set; }
        public decimal Medium { get; set; }
        public decimal Big { get; set; }
        public decimal PricePerGram { get; set; }
        public bool IsAvailable { get; set; }
        public string? Image { get; set; }
    }
}
