using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Entities
{
    public class Pizza
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "В названии пиццы не может быть более 100 символов")] 
        public string Name { get; set; }
        [StringLength(1000, ErrorMessage = "В описании пиццы не может быть больше 1000 символов")]
        public string Description { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        [Required]
        public string Image { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}
