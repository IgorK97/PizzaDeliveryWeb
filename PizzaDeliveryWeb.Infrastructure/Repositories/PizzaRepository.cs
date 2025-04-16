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
    public class PizzaRepository : IPizzaRepository
    {
        private readonly PizzaDeliveringContext _context;

        public PizzaRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }
        public async Task<Pizza> GetPizzaByIdAsync(int id)
        {
            return await _context.Pizzas.Include(t => t.Ingredients).FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<IEnumerable<Pizza>> GetPizzasAsync(int lastId=0, int pageSize=10,
            bool? isAvailable = null)
        {

            var query = _context.Pizzas.Include(t => t.Ingredients).Where(p=>!p.IsDeleted).AsQueryable();

            if (isAvailable.HasValue)
                query = query.Where(p => p.IsAvailable == isAvailable.Value);

            return await query.OrderBy(p => p.Id)
                .Where(p => p.Id > lastId)
                .Take(pageSize)
                .ToListAsync();

            //return await _context.Pizzas
            //    .Include(p => p.Ingredients)
            //    .ToListAsync();



            //return await _context.Pizzas.
            //    AsNoTracking().
            //    Include(p => p.Ingredients).
            //    OrderBy(p => p.Id).
            //    Where(p => p.Id > lastId).
            //    Take(pageSize).
            //    ToListAsync();
        }

        public async Task<IEnumerable<Pizza>> GetAllPizzasIncludingDeletedAsync()
        {
            return await _context.Pizzas.Include(p => p.Ingredients)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }
        public async Task AddPizzaAsync(Pizza pizza)
        {
            _context.Pizzas.Add(pizza);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePizzaAsync(Pizza pizza)
        {
            
            _context.Entry(pizza).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task SetPizzaAvailabilityAsync(int id, bool isAvailable)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if(pizza!=null && !pizza.IsDeleted)
            {
                pizza.IsAvailable = isAvailable;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePizzaAsync(int id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza != null)
            {
                //_context.Pizzas.Remove(pizza);
                pizza.IsDeleted = true;
                pizza.IsAvailable = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestorePizzaAsync(int id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza != null)
            {
                pizza.IsAvailable = false;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Ingredient>> GetIngredientsByPizzaIdAsync(int pizzaId)
        {
            return await _context.Ingredients.Include(t => t.Pizzas).Where(t => t.Pizzas.Any(p => p.Id == pizzaId)).ToListAsync();
        }
    }
}
