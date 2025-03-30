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

    public class StatusRepository : IStatusRepository
    {
        private readonly PizzaDeliveringContext _context;

        public StatusRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }
        public async Task<DelStatus> GetStatusByDescriptionAsync(string description)
        {
            var status = await _context.DelStatuses
                .Where(s => s.Description == description)
                .FirstOrDefaultAsync();
            if (status == null)
                throw new ArgumentException("Статус заказа не найден");

            return status;
        }
    }
}
