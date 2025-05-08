using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaDeliveryWeb.Infrastructure.Data;

namespace PizzaDeliveryWeb.Infrastructure.Repositories
{
    /// <summary>
    /// Реализация паттерна "Единица работы" (Unit of Work) для управления репозиториями.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PizzaDeliveringContext _context;
        private IDbContextTransaction _transaction;

        /// <summary>
        /// Создаёт экземпляр UnitOfWork с внедрённым контекстом базы данных.
        /// Инициализирует репозитории.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>

        public UnitOfWork(PizzaDeliveringContext context)
        {
            _context = context;
            Orders = new OrderRepository(context);
            OrderLines = new OrderLineRepository(context);
            Deliveries = new DeliveryRepository(context);
            PizzaSizes = new PizzaSizeRepository(context);
            Ingredients = new IngredientRepository(context);
            Pizzas = new PizzaRepository(context);
            Statuses = new StatusRepository(context);
        }

        public IOrderRepository Orders { get; }
        public IOrderLineRepository OrderLines { get; }
        public IDeliveryRepository Deliveries { get; }
        public IPizzaSizeRepository PizzaSizes { get; }
        public IIngredientRepository Ingredients { get; }
        public IPizzaRepository Pizzas { get; }
        public IStatusRepository Statuses { get; }

        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }




    }
}
