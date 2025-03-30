using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaDeliveryWeb.Infrastructure.Data;

namespace PizzaDeliveryWeb.Infrastructure.Repositories
{
    public class PizzaSizeRepository : IPizzaSizeRepository
    {
        private readonly PizzaDeliveringContext _context;

        public PizzaSizeRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }
        public async Task<PizzaSize> GetPizzaSizeByIdAsync(int id)
        {
            var status = await _context.PizzaSizes.Where(ps => ps.Id == id).FirstOrDefaultAsync();
            return status;
        }
        public async Task<List<PizzaSize>> GetPizzaSizesAsync()
        {
            return await _context.PizzaSizes.ToListAsync();
        }
    }
}
