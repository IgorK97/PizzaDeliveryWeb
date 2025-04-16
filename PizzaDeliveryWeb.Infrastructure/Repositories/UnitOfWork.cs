using System;
using System.Collections.Generic;
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
        }

        public IOrderRepository Orders { get; }
        public IOrderLineRepository OrderLines { get; }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        public void Dispose() => _context?.Dispose();
    }
}
