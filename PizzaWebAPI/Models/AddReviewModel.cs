namespace PizzaDeliveryWeb.API.Models
{
    public class AddReviewModel
    {
        public int OrderId { get; set; }
        public string ClientId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
    }
}
