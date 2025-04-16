using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class UpdatePizzaDto
    {
        [Required] public int Id { get; set; } // Основное отличие
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public List<int> Ingredients { get; set; }
        public string? Image { get; set; } // Base64 или null
    }
}
