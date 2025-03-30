using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Entities
{
    public class Ingredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "В названии ингредиента должно быть не более 100 символов")] 
        public string Name { get; set; }
        [StringLength(1000, ErrorMessage = "В описании ингредиента не может быть более 1000 символов")]
        public string Description { get; set; }
        [Required]
        [Column(TypeName ="decimal(10,2)")]
        public decimal Small { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Medium { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Big { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal PricePerGram { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        [Required]
        public string Image { get; set; }

        public ICollection<Pizza> Pizzas { get; set; } = new List<Pizza>();
        public ICollection<OrderLine> OrderLines { get; set; }=new List<OrderLine>();
    }
}
