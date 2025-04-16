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
            return await _context.Orders
                .Include(p => p.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o => o.Deliveries)
                .Include(o => o.DelStatus)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        //public async Task<List<OrderLine>> GetOrderLinesByOrderIdAsync(int orderId)
        //{
        //    return await _context.OrderLines.Where(t => t.OrderId == orderId).ToListAsync();
        //}
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
            return await _context.OrderLines
                .AsNoTracking()
                .Include(ol => ol.Order)
                .Include(ol=>ol.Pizza)
                    .ThenInclude(p=>p.Ingredients)
                    .Where(ol=>ol.Order.DelStatusId==(int)OrderStatusEnum.NotPlaced &&
                    ol.Pizza.Ingredients.Any(i=>i.Id==ingredientId))
                .ToListAsync();

            //var necOrders = result.Where(ol => 
            //    ol.Order.DelStatusId == (int)OrderStatusEnum.NotPlaced &&
            //    ol.Ingredients.Any(oli=>oli.Id==ingredientId));
            //return necOrders;
        }


        //public async Task<List<Delivery>> GetDeliveriesByOrderIdAsync(int orderId)
        //{
        //    return await _context.Deliveries.Where(t => t.OrderId == orderId).ToListAsync();
        //}

        public async Task<IEnumerable<Order>> GetOrdersAsync(int ?lastId = null,
            int pageSize=10)
        {
            var query = _context.Orders.AsNoTracking()
                .OrderBy(o => o.Id)
                .Take(pageSize);
            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id > lastId.Value);
            }
            return await query.ToListAsync();

            //return await _context.Orders.Include(p => p.OrderLines).ThenInclude(ol => ol.Pizza).ThenInclude(p => p.Ingredients).
            //    Include(o=>o.Deliveries).ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId,
            int? lastId=null, int pageSize=10)
        {
            var query = _context.Orders.AsNoTracking()
                .Where(o => o.ClientId == userId)
                .OrderBy(o => o.Id)
                .Take(pageSize);

            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id > lastId.Value);
            }

            return await query.ToListAsync();

            //return await _context.Orders.Where(o => o.ClientId == userId).Include(p=>p.OrderLines)
            //    .ThenInclude(ol=>ol.Pizza).ThenInclude(p=>p.Ingredients).Include(o=>o.Deliveries).ToListAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                
            }
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                //if (order.OrderLines.Any())
                //{
                //    throw new InvalidOperationException("Заказ не может быть удален, потому что он связан со строками.");
                //}

                //_context.Orders.Remove(order);

                order.DelStatusId = (int)OrderStatusEnum.IsCancelled;
                await _context.SaveChangesAsync();
            }
        }
    }
}
