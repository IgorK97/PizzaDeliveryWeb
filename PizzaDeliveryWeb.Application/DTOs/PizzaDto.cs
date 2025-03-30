using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class PizzaDto
    {
        
        public int Id { get; set; }
       
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }

        public string Image { get; set; }
        public ICollection<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    }
}
