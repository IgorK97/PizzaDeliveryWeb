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
    public class DeliveryRepository : IDeliveryRepository
    {

        private readonly PizzaDeliveringContext _context;

        public DeliveryRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }
        public async Task AddDeliveryAsync(Delivery delivery)
        {
            _context.Deliveries.Add(delivery);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDeliveryAsync(int id)
        {
            var delivery = await _context.Deliveries.FirstOrDefaultAsync(p => p.Id == id);
            if (delivery != null)
            {
                
                _context.Deliveries.Remove(delivery);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesAsync()
        {
            return await _context.Deliveries.Include(d=>d.Order).ThenInclude(o => o.OrderLines).
                ThenInclude(ol => ol.Pizza).ThenInclude(p => p.Ingredients).Include(d => d.Order).
                ThenInclude(o => o.OrderLines).ThenInclude(ol => ol.Ingredients).ToListAsync();

        }

        public async Task<Delivery> GetDeliveryByIdAsync(int id)
        {
            //Возврат доставки вместе с заказом (а в заказе строке заказа, в каждой строке заказа - 
            //дополнительные ингредиенты и пицца, а для каждой пиццы - уже ее собственные ингредиенты)
            return await _context.Deliveries.Include(d=>d.Order).ThenInclude(o=>o.OrderLines).
                ThenInclude(ol=>ol.Pizza).ThenInclude(p=>p.Ingredients).Include(d=>d.Order).
                ThenInclude(o=>o.OrderLines).ThenInclude(ol=>ol.Ingredients).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByOrderIdAsync(int orderId)
        {
            return await _context.Deliveries.Include(d => d.Order).ThenInclude(o => o.OrderLines).
                ThenInclude(ol => ol.Pizza).ThenInclude(p => p.Ingredients).Include(d => d.Order).
                ThenInclude(o => o.OrderLines).ThenInclude(ol => ol.Ingredients).Where(d => d.OrderId == orderId).ToListAsync();
        }

        public async Task UpdateDeliveryAsync(Delivery delivery)
        {
            _context.Entry(delivery).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
