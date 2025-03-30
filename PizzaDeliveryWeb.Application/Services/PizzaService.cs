using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class PizzaService
    {
        private readonly IPizzaRepository _pizzaRepository;
        private readonly IIngredientRepository _ingrRepository;

        public PizzaService(IPizzaRepository pizzaRepository, IIngredientRepository ingrRepository)
        {
            _pizzaRepository = pizzaRepository;
            _ingrRepository = ingrRepository;
        }

        public async Task<IEnumerable<PizzaDto>> GetPizzasAsync()
        {
            var pizzas = await _pizzaRepository.GetPizzasAsync();
            return pizzas.Select(p => {
                List<IngredientDto> addedIngrs = new List<IngredientDto>();
                foreach(Ingredient i in p.Ingredients)
                {
                    addedIngrs.Add(new IngredientDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        Small = i.Small,
                        Medium = i.Medium,
                        Big = i.Big,
                        PricePerGram = i.PricePerGram
                    });
                }
                return new PizzaDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Ingredients=addedIngrs,
                    Image=p.Image,
                    IsAvailable=p.IsAvailable
                };
            });
        }

        public async Task<PizzaDto> GetPizzaByIdAsync(int id)
        {
            var pizza = await _pizzaRepository.GetPizzaByIdAsync(id);
            if (pizza == null) return null;
            return new PizzaDto
            {
                Id = pizza.Id,
                Name = pizza.Name,
                Description = pizza.Description,
                IsAvailable = pizza.IsAvailable,
                Image = pizza.Image,
                Ingredients = pizza.Ingredients.Select(t => new IngredientDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Small = t.Small,
                    Medium = t.Medium,
                    Big = t.Big,
                    PricePerGram = t.PricePerGram
                }).ToList()
            };
        }

        public async Task<List<IngredientDto>> GetIngredintsForPizzaAsync(int pizzaId)
        {
            var ingrs = await _pizzaRepository.GetIngredientsByPizzaIdAsync(pizzaId);

            if (ingrs == null || ingrs.Count == 0)
                return null;

            return ingrs.Select(t => new IngredientDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Small = t.Small,
                Medium = t.Medium,
                Big = t.Big,
                PricePerGram = t.PricePerGram
            }).ToList();
        }



        public async Task AddPizzaAsync(CreatePizzaDto pizzaDto)
        {
            try
            {
                var pizza = new Pizza
                {
                    Id=0,
                    Name = pizzaDto.Name,
                    Description = pizzaDto.Description,
                    IsAvailable=pizzaDto.IsAvailable,
                    Image = pizzaDto.Image
                };
                List<Ingredient> addedIngredients = new List<Ingredient>();
                foreach (int a in pizzaDto.Ingredients)
                {
                    addedIngredients.Add(await _ingrRepository.GetIngredientByIdAsync(a));
                }
                pizza.Ingredients = addedIngredients;
                await _pizzaRepository.AddPizzaAsync(pizza);
                pizzaDto.Id = pizza.Id;
            }
            catch(Exception ex)
            {
                throw new ApplicationException($"Error adding pizza: {ex.Message}", ex);
            }
        }

        public async Task UpdatePizzaAsync(PizzaDto pizzaDto)
        {
            var pizza = await _pizzaRepository.GetPizzaByIdAsync(pizzaDto.Id);
            if (pizza != null)
            {
                pizza.Name = pizzaDto.Name;
                pizza.Description = pizzaDto.Description;
                pizza.Image = pizzaDto.Image;
                pizza.IsAvailable = pizzaDto.IsAvailable;
                //ingredients
                await _pizzaRepository.UpdatePizzaAsync(pizza);
            }
        }

        public async Task DeletePizzaAsync(int id)
        {
            try
            {
                await _pizzaRepository.DeletePizzaAsync(id);
            }
            catch (InvalidOperationException ex)
            {
               
                throw new ApplicationException($"Error deleting pizza: {ex.Message}", ex);
            }
        }
    }
}
