using System.Collections;
using PizzaDeliveryWeb.Application.DTOs;

namespace PizzaDeliveryWeb.API.Models
{
    public class ResultGetPizzas
    {
        public IEnumerable<PizzaDto>? Items { get; set; }
        public int LastId { get; set; }
        public bool HasMore { get; set; }
    }
}
