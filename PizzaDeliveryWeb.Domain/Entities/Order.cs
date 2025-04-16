using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace PizzaDeliveryWeb.Domain.Entities
{
    public enum PizzaSizeEnum
    {
        Small = 1,
        Medium = 2,
        Big = 3
    };
    public enum OrderStatusEnum
    {
        NotPlaced = 1,
        IsBeingFormed,
        IsBeingPrepared,
        IsBeingTransferred,
        HasBeenTransferred,
        IsCancelled,
        IsDelivered,
        IsNotDelivered
    }
    public partial class Order
    {
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string ClientId { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Price { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Weight { get; set; }
        public string Address { get; set; }
        [Required]
        public int DelStatusId { get; set; }
        public string? ManagerId { get; set; }
        
        
        public DateTime? OrderTime { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? CancellationTime { get; set; }
        
        public virtual User Client { get; set; }
        public virtual User? Manager { get; set; }

        public virtual DelStatus DelStatus { get; set; }
        public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
