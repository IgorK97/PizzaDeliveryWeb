using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IDeliveryRepository
    {
        Task<Delivery> GetDeliveryByIdAsync(int id);
        Task<IEnumerable<Delivery>> GetDeliveriesAsync();
        Task<Delivery> GetDeliveryByOrderIdAsync(int id);
        Task AddDeliveryAsync(Delivery delivery);
        Task UpdateDeliveryAsync(Delivery delivery);
        Task DeleteDeliveryAsync(int id);
    }
}
