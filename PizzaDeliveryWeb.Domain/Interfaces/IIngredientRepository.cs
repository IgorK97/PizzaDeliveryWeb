using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IIngredientRepository
    {
        Task<Ingredient> GetIngredientByIdAsync(int id);
        Task<IEnumerable<Ingredient>> GetIngredientsAsync();

        Task<List<Ingredient>> GetIngredientsByIdsAsync(IEnumerable<int> ids);
        Task AddIngredientAsync(Ingredient ingredient);
        Task UpdateIngredientAsync(Ingredient ingredient);
        Task DeleteIngredientAsync(int id);
    }
}
