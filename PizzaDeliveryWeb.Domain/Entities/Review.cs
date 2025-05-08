using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Entities
{
    /// <summary>
    /// Сущность, представляет отзыв клиента о заказе
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Уникальный идентификатор отзыва
        /// </summary>
        /// <remarks>
        /// Первичный ключ, генерируемый базой данных
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор связанного заказа
        /// </summary>
        /// <value>Внешний ключ для <see cref="Order"/></value>
        [Required]
        public int OrderId { get; set; }
        /// <summary>
        /// Идентификатор клиента, оставившего отзыв
        /// </summary>
        /// <value>Внешний ключ для <see cref="User"/></value>
        [Required]
        public string ClientId { get; set; }
        /// <summary>
        /// Оценка заказа (обязательное поле)
        /// </summary>
        /// <value>Числовое значение от 1 до 5</value>
        [Required]
        public int Rating { get; set; }
        /// <summary>
        /// Текст отзыва (необязательное поле)
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// Дата и время создания отзыва
        /// </summary>
        /// <value>Автоматически устанавливается при создании</value>
        [Required]
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Навигационное свойство для связанного заказа
        /// </summary>
        public Order Order { get; set; }
        /// <summary>
        /// Навигационное свойство для клиента-автора отзыва
        /// </summary>
        public User Client { get; set; }
    }
}
