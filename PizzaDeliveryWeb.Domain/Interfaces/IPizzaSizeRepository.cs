using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;


namespace PizzaDeliveryWeb.Domain.Interfaces
{
    public interface IPizzaSizeRepository
    {
        Task<PizzaSize> GetPizzaSizeByIdAsync(int id);
        Task<List<PizzaSize>> GetPizzaSizesAsync();

        Task<PizzaSize> GetPizzaSizeByNameAsync(string name);
    }
}
