using Microsoft.EntityFrameworkCore;

namespace PizzaDeliveryWeb.API.Models
{
    public class SetDeliveryModel
    {
        public int OrderId { get; set; }
        public bool Status { get; set; }
        public string Comment { get; set; }
    }
}
