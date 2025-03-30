using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaDeliveryWeb.Infrastructure.Data;

namespace PizzaDeliveryWeb.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PizzaDeliveringContext _context;

        public OrderRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.Include(p => p.OrderLines).ThenInclude(ol => ol.Pizza).ThenInclude(p => p.Ingredients)
                .Include(o => o.Deliveries).Include(o => o.DelStatus).Include(o =>o.Client).Include(o=>o.Manager)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<OrderLine>> GetOrderLinesByOrderIdAsync(int orderId)
        {
            return await _context.OrderLines.Where(t => t.OrderId == orderId).ToListAsync();
        }
        public async Task<IEnumerable<OrderLine>> GetNotPlacedOrderLinesWithIngredientAsync(int ingredientId)
        {
            //    return await _context.OrderLines
            //.Include(ol => ol.Order).
            //.Include(ol => ol.Ingredients)
            //    .ThenInclude(oli => oli.Ingredient)
            //.Where(ol =>
            //    ol.Order.Status == OrderStatus.Pending &&
            //    ol.Ingredients.Any(oli => oli.IngredientId == ingredientId))
            //.ToListAsync();
            var result = await _context.OrderLines.Include(ol => ol.Order).ToListAsync();
            var necOrders = result.Where(ol => 
                ol.Order.DelStatusId == (int)OrderStatusEnum.NotPlaced &&
                ol.Ingredients.Any(oli=>oli.Id==ingredientId));
            return necOrders;
        }


        public async Task<List<Delivery>> GetDeliveriesByOrderIdAsync(int orderId)
        {
            return await _context.Deliveries.Where(t => t.OrderId == orderId).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders.Include(p => p.OrderLines).ThenInclude(ol => ol.Pizza).ThenInclude(p => p.Ingredients).
                Include(o=>o.Deliveries).ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders.Where(o => o.ClientId == userId).Include(p=>p.OrderLines)
                .ThenInclude(ol=>ol.Pizza).ThenInclude(p=>p.Ingredients).Include(o=>o.Deliveries).ToListAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.Include(p => p.OrderLines).FirstOrDefaultAsync(p => p.Id == id);
            if (order != null)
            {
                if (order.OrderLines.Any())
                {
                    throw new InvalidOperationException("Заказ не может быть удален, потому что он связан со строками.");
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}
