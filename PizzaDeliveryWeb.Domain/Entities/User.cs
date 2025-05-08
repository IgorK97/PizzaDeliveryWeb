using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PizzaDeliveryWeb.Domain.Entities
{
    /// <summary>
    /// Представляет учетную запись пользователя в системе
    /// </summary>
    /// <remarks>
    /// Наследует базовую функциональность пользователя от <see cref="IdentityUser"/>
    /// </remarks>
    public class User : IdentityUser
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// отчество (не обязательно)
        /// </summary>
        public string? Surname { get; set; }
        /// <summary>
        /// Адрес доставки пользователя (необязательное поле)
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Список заказов, связанных с пользователем
        /// </summary>
        /// <value>Коллекция объектов <see cref="Order"/></value>

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        /// <summary>
        /// Список доставок, связанных с пользователем
        /// </summary>
        /// <value>Коллекция объектов <see cref="Delivery"/></value>
        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        /// <summary>
        /// Список отзывов, оставленных пользователем
        /// </summary>
        /// <value>Коллекция объектов <see cref="Review"/></value>
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
