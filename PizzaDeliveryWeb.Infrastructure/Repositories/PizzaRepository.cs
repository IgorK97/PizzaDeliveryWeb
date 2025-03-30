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
        public async Task<IEnumerable<Pizza>> GetPizzasAsync()
        {
            return await _context.Pizzas
                .Include(p => p.Ingredients)
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
        public async Task DeletePizzaAsync(int id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza != null)
            {
                _context.Pizzas.Remove(pizza);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Ingredient>> GetIngredientsByPizzaIdAsync(int pizzaId)
        {
            return await _context.Ingredients.Include(t => t.Pizzas).Where(t => t.Pizzas.Any(p => p.Id == pizzaId)).ToListAsync();
        }
    }
}
