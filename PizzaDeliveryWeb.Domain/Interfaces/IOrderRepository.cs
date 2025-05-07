using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderByIdAsync(int id);
        //Task<List<OrderLine>> GetOrderLinesByOrderIdAsync(int orderId);
        //Task<List<Delivery>> GetDeliveriesByOrderIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersAsync(int? lastId=null, int pageSize=10);
        Task<IEnumerable<Order>> GetActiveOrdersAsync(int? lastId = null, int pageSize = 10);
        Task<IEnumerable<Order>> GetOrdersByClientIdAsync(string clientId, int? lastId= null, int pageSize = 50);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId,
            int? lastId = null, int pageSize=10);
        Task AddOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task CancelOrderAsync(int id);
        Task<IEnumerable<OrderLine>> GetNotPlacedOrderLinesWithIngredientAsync(int ingredientId);

        Task<Order> GetOrderWithDeliveryAsync(int id);

        Task<IEnumerable<Order>> GetOrdersByStatusAsync(
            int statusId,
            int? lastId,
            int pageSize);

        Task<Order> GetCartAsync(string clientId);
        //Task<Order> UpdateCartAsync(Order order);
    }
}
