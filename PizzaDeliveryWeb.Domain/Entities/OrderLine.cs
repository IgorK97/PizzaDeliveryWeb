using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaDeliveryWeb.Domain.Entities
{
    public class OrderLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int OrderId;
        [Required]
        public int PizzaId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Price { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Weight { get; set; }

        [Required]
        public int Quantity { get; set; }
        [Required]
        public bool Custom { get; set; }
        [Required]
        public int PizzaSizeId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Pizza Pizza { get; set; }
        public virtual PizzaSize PizzaSize { get; set; }
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}
