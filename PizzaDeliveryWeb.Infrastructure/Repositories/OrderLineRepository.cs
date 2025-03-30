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
    public class OrderLineRepository : IOrderLineRepository
    {
        private readonly PizzaDeliveringContext _context;

        public OrderLineRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }

        public async Task<OrderLine> GetOrderLineByIdAsync(int id)
        {
            return await _context.OrderLines.Include(ol=>ol.PizzaSize).Include(t => t.Pizza).ThenInclude(p=>p.Ingredients).Include(ol=>ol.Order).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<OrderLine>> GetOrderLinesAsync()
        {
            return await _context.OrderLines
                .Include(t => t.Pizza).ThenInclude(p => p.Ingredients)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderLine>> GetOrderLinesByOrderIdAsync(int orderId)
        {
            return await _context.OrderLines
                .Where(t => t.OrderId == orderId).Include(ol=>ol.PizzaSize)
                .Include(t => t.Pizza).ThenInclude(p => p.Ingredients)
                .ToListAsync();
        }


        

        public async Task AddOrderLineAsync(OrderLine oline)
        {
            var orderExists = await _context.Orders.AnyAsync(p => p.Id == oline.OrderId);

            if (!orderExists)
            {
                throw new ArgumentException($"Заказ с ID {oline.OrderId} не существует.");
            }

            _context.OrderLines.Add(oline);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderLineAsync(OrderLine oline)
        {
            var orderExists = await _context.Orders.AnyAsync(p => p.Id == oline.OrderId);

            if (!orderExists)
            {
                throw new ArgumentException($"Заказ с ID {oline.OrderId} не существует.");
            }

            _context.Entry(oline).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderLineAsync(int id)
        {
            var task = await _context.OrderLines.FindAsync(id);
            if (task != null)
            {
                _context.OrderLines.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}
