using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Entities
{
    /// <summary>
    /// Сущность ингредиента в базе данных
    /// </summary>
    public class Ingredient
    {
        /// <summary>
        /// Идентификатор ингредиента
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Наименование ингредиента
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "В названии ингредиента должно быть не более 100 символов")] 
        public string Name { get; set; }
        /// <summary>
        /// Описание ингредиента
        /// </summary>
        [StringLength(1000, ErrorMessage = "В описании ингредиента не может быть более 1000 символов")]
        public string Description { get; set; }
        /// <summary>
        /// Количество (в граммах) вхождения данного ингредиента в состав пиццы маленького размера 
        /// (для всех одинаков, все пиццы по шаблону готовятся)
        /// </summary>
        [Required]
        [Column(TypeName ="decimal(10,2)")]
        public decimal Small { get; set; }

        /// <summary>
        /// Количество (в граммах) вхождения данного ингредиента в состав пиццы среднего размера 
        /// (для всех одинаков)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Medium { get; set; }

        /// <summary>
        /// Количество (в граммах) вхождения данного ингредиента в состав пиццы большого размера 
        /// (для всех одинаков)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Big { get; set; }


        /// <summary>
        /// Цена за грамм (в рублях) для данного ингредиента
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal PricePerGram { get; set; }

        /// <summary>
        /// Признак (флаг), присутствует ли ингредиент в ассортименте на данный момент
        /// </summary>
        [Required]
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Относительный путь к изображению ингредиента
        /// </summary>
        [Required]
        public string Image { get; set; }


        /// <summary>
        /// Флаг, удален ли ингердиент из ассортимента
        /// </summary>
        public bool IsDeleted { get; set; } = false;


        /// <summary>
        /// Коллекция пицц, в которые входит ингредиент
        /// </summary>
        public ICollection<Pizza> Pizzas { get; set; } = new List<Pizza>();


        /// <summary>
        /// Коллекция строк заказа, в которые ингердиент был доабвлен дополнительно пользователем
        /// </summary>
        public ICollection<OrderLine> OrderLines { get; set; }=new List<OrderLine>();
    }
}
