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
    public class IngredientRepository : IIngredientRepository
    {
        private readonly PizzaDeliveringContext _context;

        public IngredientRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }

        public async Task<Ingredient> GetIngredientByIdAsync(int id)
        {
            return await _context.Ingredients.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientsAsync()
        {
            return await _context.Ingredients
                .ToListAsync();
        }

        public async Task AddIngredientAsync(Ingredient ingredient)
        {

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateIngredientAsync(Ingredient ingredient)
        {

            _context.Entry(ingredient).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteIngredientAsync(int id)
        {
            var ingr = await _context.Ingredients.FindAsync(id);
            if (ingr != null)
            {
                _context.Ingredients.Remove(ingr);
                await _context.SaveChangesAsync();
            }
        }
    }
}
