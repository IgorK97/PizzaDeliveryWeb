using PizzaDeliveryWeb.Application.DTOs;

namespace PizzaDeliveryWeb.API.Models
{
    public class CartSubmitResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CartDto UpdatedCart { get; set; }
    }
}
