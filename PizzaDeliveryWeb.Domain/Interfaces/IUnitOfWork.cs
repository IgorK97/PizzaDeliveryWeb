using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IOrderLineRepository OrderLines { get; }
        IDeliveryRepository Deliveries { get; }
        IPizzaSizeRepository PizzaSizes { get; }
        IIngredientRepository Ingredients { get; }
        IPizzaRepository Pizzas { get; }
        IStatusRepository Statuses { get; }
        Task<int> Save();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
