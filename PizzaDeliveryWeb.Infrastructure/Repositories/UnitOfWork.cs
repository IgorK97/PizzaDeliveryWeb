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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PizzaDeliveringContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(PizzaDeliveringContext context)
        {
            _context = context;
            Orders = new OrderRepository(context);
            OrderLines = new OrderLineRepository(context);
            Deliveries = new DeliveryRepository(context);
            PizzaSizes = new PizzaSizeRepository(context);
            Ingredients = new IngredientRepository(context);
            Pizzas = new PizzaRepository(context);
        }

        public IOrderRepository Orders { get; }
        public IOrderLineRepository OrderLines { get; }
        public IDeliveryRepository Deliveries { get; }
        public IPizzaSizeRepository PizzaSizes { get; }
        public IIngredientRepository Ingredients { get; }
        public IPizzaRepository Pizzas { get; }

        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            //return transaction as IDbTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            await Save();
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
        }
        private bool disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                this.disposed = true;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        public void Dispose()
        {
            //_context?.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
