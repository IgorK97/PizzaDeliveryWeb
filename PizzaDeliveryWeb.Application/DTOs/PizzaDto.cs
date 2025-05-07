using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    //[JsonConverter(typeof(JsonStringEnumConverter))]
    //[JsonConverter(typeof(JsonNumberEnumConverter))]
    public enum PizzaSizeEnum
    {
        Small=1,
        Medium=2,
        Big=3
    }
    public class PizzaDto
    {
        
        public int Id { get; set; }
       
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }

        public string Image { get; set; }

        public Dictionary<int, decimal> Prices { get; set; } = new Dictionary<int,decimal>();

        public Dictionary<int, decimal> Weights { get; set; } = new Dictionary<int, decimal>();
        public ICollection<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    }
}
