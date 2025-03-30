using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Entities
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public string ClientId { get; set; }
        [Required]
        public int Rating { get; set; }
        public string? Content { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public Order Order { get; set; }
        public User Client { get; set; }
    }
}
