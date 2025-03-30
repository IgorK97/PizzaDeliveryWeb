using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IOrderLineRepository
    {
        Task<OrderLine> GetOrderLineByIdAsync(int id);
        Task<IEnumerable<OrderLine>> GetOrderLinesAsync();
        Task AddOrderLineAsync(OrderLine oline);
        Task UpdateOrderLineAsync(OrderLine oline);
        Task DeleteOrderLineAsync(int id);
        Task<IEnumerable<OrderLine>> GetOrderLinesByOrderIdAsync(int orderId);

    }
}
