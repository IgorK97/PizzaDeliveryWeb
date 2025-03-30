using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IPizzaRepository
    {
        Task<Pizza> GetPizzaByIdAsync(int id);
        Task<IEnumerable<Pizza>> GetPizzasAsync();
        Task<List<Ingredient>> GetIngredientsByPizzaIdAsync(int pizzaId);
        Task AddPizzaAsync(Pizza pizza);
        Task UpdatePizzaAsync(Pizza pizza);
        Task DeletePizzaAsync(int id);
    }
}
