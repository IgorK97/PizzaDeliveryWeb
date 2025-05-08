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
    /// Сущность пиццу в ассортименте
    /// </summary>
    public class Pizza
    {
        /// <summary>
        /// Уникальный идентификатор пиццы
        /// </summary>
        /// <remarks>
        /// Первичный ключ, автоматически генерируемый базой данных
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Название пиццы
        /// </summary>
        /// <value>
        /// Должно содержать от 1 до 100 символов
        /// </value>
        [Required]
        [StringLength(100, ErrorMessage = "В названии пиццы не может быть более 100 символов")] 
        public string Name { get; set; }
        /// <summary>
        /// Описание пиццы
        /// </summary>
        /// <value>
        /// Необязательное поле, максимальная длина - 1000 символов
        /// </value>
        [StringLength(1000, ErrorMessage = "В описании пиццы не может быть больше 1000 символов")]
        public string Description { get; set; }
        /// <summary>
        /// Флаг доступности пиццы для заказа
        /// </summary>
        /// <value>
        /// true - доступна в меню, false - временно недоступна
        /// </value>
        [Required]
        public bool IsAvailable { get; set; }
        /// <summary>
        /// Путь к изображению пиццы
        /// </summary>
        /// <value>
        /// Относительный URL изображения
        /// </value>
        [Required]
        public string Image { get; set; }
        /// <summary>
        /// Флаг мягкого удаления
        /// </summary>
        /// <value>
        /// true - помечена как удаленная, false - активная запись (по умолчанию)
        /// </value>
        public bool IsDeleted { get; set; } = false;
        /// <summary>
        /// Список позиций заказов, связанных с этой пиццей
        /// </summary>
        /// <value>Коллекция объектов <see cref="OrderLine"/></value>

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        /// <summary>
        /// Ингредиенты, входящие в состав пиццы
        /// </summary>
        /// <value>Коллекция объектов <see cref="Ingredient"/></value>
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}
