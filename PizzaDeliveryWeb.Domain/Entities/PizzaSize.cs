using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaDeliveryWeb.Domain.Entities;

public partial class PizzaSize
{
    //public int Id { get; set; }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [StringLength(30, ErrorMessage = "В названии размера пиццы не может быть более 30 символов")]
    public string Name { get; set; } = null!;
    [Required]
    [Column(TypeName = "decimal(10,2)")]

    public decimal Price { get; set; }
    [Required]
    [Column(TypeName = "decimal(10,2)")]

    public decimal Weight { get; set; }

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
