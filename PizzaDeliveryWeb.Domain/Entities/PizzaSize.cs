using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaDeliveryWeb.Domain.Entities;

/// <summary>
/// Сущность размера заготовки пиццы
/// </summary>
public partial class PizzaSize
{
    //public int Id { get; set; }


    /// <summary>
    /// Идентификатор размера пиццы
    /// </summary>


    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    /// <summary>
    /// Наименование заготовки
    /// </summary>
    [Required]
    [StringLength(30, ErrorMessage = "В названии размера пиццы не может быть более 30 символов")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Цена заготовки
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
    /// <summary>
    /// Вес заготовки
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]

    public decimal Weight { get; set; }
    /// <summary>
    /// Коллекция строк (позиций) заказа с пиццами такого размера
    /// </summary>

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
