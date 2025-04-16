using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PizzaSize
    {
        Small,
        Medium,
        Big
    }
    public class PizzaDto
    {
        
        public int Id { get; set; }
       
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }

        public string Image { get; set; }

        public Dictionary<PizzaSize, decimal> Prices { get; set; } = new Dictionary<PizzaSize,decimal>();

        public Dictionary<PizzaSize, decimal> Weights { get; set; } = new Dictionary<PizzaSize, decimal>();
        public ICollection<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    }
}
