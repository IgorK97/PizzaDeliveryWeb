using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int PizzaId { get; set; }
        public string PizzaName { get; set; }
        public string PizzaImage { get; set; }
        public string PizzaSize { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal ItemWeight { get; set; }
        public int Quantity { get; set; }
        
        //public List<IngredientDto> DefaultIngredients { get; set; } = new List<IngredientDto>();
        //public List<IngredientDto> AvailableExtras { get; set; } = new List<IngredientDto>();,
        public List<int> DefaultIngredientIds { get; set; }
        public List<int> AddedIngredientIds { get; set; }
        
    }
}
