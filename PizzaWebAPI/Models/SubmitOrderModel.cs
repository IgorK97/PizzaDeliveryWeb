namespace PizzaDeliveryWeb.API.Models
{
    public class SubmitOrderModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Address { get; set; }

    }
}
