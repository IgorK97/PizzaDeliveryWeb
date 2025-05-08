using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaDeliveryWeb.Domain.Entities
{
    /// <summary>
    /// Представляет строку заказа, содержащую информацию о конкретной пицце,
    /// её размере, стоимости, весе, количестве и ингредиентах.
    /// </summary>
    public class OrderLine
    {
        /// <summary>
        /// Уникальный идентификатор строки заказа.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор заказа, к которому относится данная строка.
        /// </summary>
        [Required]
        public int OrderId;
        /// <summary>
        /// Идентификатор пиццы, указанной в строке заказа.
        /// </summary>
        [Required]
        public int PizzaId { get; set; }

        /// <summary>
        /// Цена пиццы в данной строке заказа.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Price { get; set; }

        /// <summary>
        /// Вес пиццы в данной строке заказа.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Weight { get; set; }

        /// <summary>
        /// Количество единиц пиццы в данной строке заказа.
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Указывает, является ли пицца кастомной (с пользовательскими ингредиентами).
        /// </summary>
        [Required]
        public bool Custom { get; set; }

        /// <summary>
        /// Идентификатор выбранного размера пиццы.
        /// </summary>
        [Required]
        public int PizzaSizeId { get; set; }


        /// <summary>
        /// Заказ, к которому принадлежит данная строка.
        /// </summary>
        public virtual Order Order { get; set; }

        /// <summary>
        /// Пицца, указанная в строке заказа.
        /// </summary>
        public virtual Pizza Pizza { get; set; }

        /// <summary>
        /// Размер пиццы, выбранный для данной строки заказа.
        /// </summary>
        public virtual PizzaSize PizzaSize { get; set; }

        /// <summary>
        /// Коллекция ингредиентов, использованных для кастомной пиццы.
        /// </summary>
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}
