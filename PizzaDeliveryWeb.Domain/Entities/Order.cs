using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace PizzaDeliveryWeb.Domain.Entities
{
    /// <summary>
    /// Перечисление размеров пиццы.
    /// </summary>
    public enum PizzaSizeEnum
    {
        Small = 1,
        Medium = 2,
        Big = 3
    };

    /// <summary>
    /// Перечисление возможных статусов заказа.
    /// </summary>
    public enum OrderStatusEnum
    {
        /// <summary>
        /// Заказ не размещён (корзина пользователя)
        /// </summary>
        NotPlaced = 1,

        /// <summary>
        /// Заказ, поступивший в пиццерию от пользователя, пока не готовится
        /// </summary>
        IsBeingFormed,

        /// <summary>
        /// Готовящийся заказ
        /// </summary>
        IsBeingPrepared,

        /// <summary>
        /// Заказ передаётся в доставку
        /// </summary>
        IsBeingTransferred,

        /// <summary>
        /// Заказ был передан службе доставки
        /// </summary>
        HasBeenTransferred,

        /// <summary>
        /// Заказ отменён
        /// </summary>
        IsCancelled,

        /// <summary>
        /// Заказ доставлен курьером успешно
        /// </summary>
        IsDelivered,


        /// <summary>
        /// Заказ не был доставлен успешно (например, клиент отказался получать
        /// в самый последний момент
        /// </summary>
        IsNotDelivered
    }

    /// <summary>
    /// Представляет заказ, оформленный клиентом.
    /// Содержит информацию о клиенте, стоимости, времени и статусе.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Уникальный идентификатор заказа.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор клиента, оформившего заказ.
        /// </summary>
        [Required]
        public string ClientId { get; set; }


        /// <summary>
        /// Общая стоимость заказа.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Price { get; set; }


        /// <summary>
        /// Общий вес заказа.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]

        public decimal Weight { get; set; }

        /// <summary>
        /// Адрес доставки.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Идентификатор статуса доставки.
        /// </summary>
        [Required]
        public int DelStatusId { get; set; }

        /// <summary>
        /// Идентификатор менеджера, обрабатывающего заказ
        /// </summary>
        public string? ManagerId { get; set; }
        //public int? DeliveryId { get; set; } = null;


        /// <summary>
        /// Время размещения заказа
        /// </summary>
        public DateTime? OrderTime { get; set; }

        /// <summary>
        /// Время подтверждения заказа менеджером
        /// </summary>
        public DateTime? AcceptedTime { get; set; }

        /// <summary>
        /// Время завершения (доставки или самовывоза) заказа.
        /// </summary>
        public DateTime? CompletionTime { get; set; }

        /// <summary>
        /// Время отмены заказа.
        /// </summary>
        public DateTime? CancellationTime { get; set; }

        /// <summary>
        /// Объект пользователя-клиента.
        /// </summary>
        public virtual User Client { get; set; }

        /// <summary>
        /// Объект пользователя-менеджера (может быть null).
        /// </summary>
        public virtual User? Manager { get; set; }


        /// <summary>
        /// Объект статуса доставки.
        /// </summary>
        public virtual DelStatus DelStatus { get; set; }


        /// <summary>
        /// Коллекция строк заказа (позиций заказа).
        /// </summary>
        public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();


        /// <summary>
        /// Доставка, связанная с заказом
        /// </summary>
        public virtual Delivery Delivery { get; set; }

        /// <summary>
        /// Коллекция отзывов, оставленных по заказу
        /// </summary>
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
