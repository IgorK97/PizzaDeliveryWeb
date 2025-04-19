using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class NewCartItemDto
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int PizzaId { get; set; }
        public string PizzaSize { get; set; }
        public int Quantity { get; set; }
        public List<int> AddedIngredientIds { get; set; } = new List<int>();
    }
}
