using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class PizzaService
    {
        private readonly IPizzaRepository _pizzaRepository;
        private readonly IIngredientRepository _ingrRepository;
        private readonly IPizzaSizeRepository _pizzaSizeRepository;

        public PizzaService(IPizzaRepository pizzaRepository, IIngredientRepository ingrRepository, IPizzaSizeRepository pizzaSizeRepository)
        {
            _pizzaRepository = pizzaRepository;
            _ingrRepository = ingrRepository;
            _pizzaSizeRepository = pizzaSizeRepository;
        }

        private decimal CalculatePrice(Pizza pizza, DTOs.PizzaSize ps, decimal basePrice)
        {
            var ingrPrice = pizza.Ingredients.Sum(pi =>
            {
                var grams = ps switch
                {
                    DTOs.PizzaSize.Small => pi.Small,
                    DTOs.PizzaSize.Medium => pi.Medium,
                    DTOs.PizzaSize.Big => pi.Big,
                    _ => throw new InvalidOperationException("Неизвестный размер пиццы")
                };
                return grams * pi.PricePerGram;
            });
            decimal totalPrice = ingrPrice + basePrice;
            return totalPrice;
        }

        private decimal CalculateWeight(Pizza pizza, DTOs.PizzaSize ps, decimal baseWeight)
        {
            var ingrWeight = pizza.Ingredients.Sum(pi =>
            {
                var grams = ps switch
                {
                    DTOs.PizzaSize.Small => pi.Small,
                    DTOs.PizzaSize.Medium => pi.Medium,
                    DTOs.PizzaSize.Big => pi.Big,
                    _ => throw new InvalidOperationException("Неизвестный размер пиццы")
                };
                return grams;
            });
            decimal totalWeight = ingrWeight + baseWeight;
            return totalWeight;
        }

        public async Task<IEnumerable<PizzaDto>> GetPizzasAsync(int lastId = 0, 
            int pageSize=10,
            bool includeUnavailable = false)
        {
            var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            var pizzas = await _pizzaRepository.GetPizzasAsync(lastId, pageSize,
                includeUnavailable? null: true);
            return pizzas.Select(p => MapToPizzaDto(p, pizzaSizes));
            //return pizzas.Select(p => {
            //    List<IngredientDto> addedIngrs = new List<IngredientDto>();
            //    foreach(Ingredient i in p.Ingredients)
            //    {
            //        addedIngrs.Add(new IngredientDto
            //        {
            //            Id = i.Id,
            //            Name = i.Name,
            //            Description = i.Description,
            //            Small = i.Small,
            //            Medium = i.Medium,
            //            Big = i.Big,
            //            PricePerGram = i.PricePerGram
            //        });
            //    }
            //    return new PizzaDto
            //    {
            //        Id = p.Id,
            //        Name = p.Name,
            //        Description = p.Description,
            //        Ingredients=addedIngrs,
            //        Image=p.Image,
            //        IsAvailable=p.IsAvailable
            //    };
            //});
        }

        private PizzaDto MapToPizzaDto(Pizza pizza, IEnumerable<Domain.Entities.PizzaSize> pizzaSizes)
        {
            var dto =  new PizzaDto
            {
                Id = pizza.Id,
                Name = pizza.Name,
                Description = pizza.Description,
                IsAvailable = pizza.IsAvailable,
                Image = pizza.Image,
                Ingredients = pizza.Ingredients?
                .Select(i => MapToIngredientDto(i))
                .ToList() ?? new List<IngredientDto>(),
                Prices = new Dictionary<DTOs.PizzaSize, decimal>(),
                Weights = new Dictionary<DTOs.PizzaSize, decimal>()
            };

            //var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            //decimal smallPrice = pizzaSizes.FirstOrDefault(p => p.Id == 1).Price;

            dto.Prices[DTOs.PizzaSize.Small] = CalculatePrice(pizza, DTOs.PizzaSize.Small, pizzaSizes.FirstOrDefault(p=>p.Name.ToLower()=="small")?.Price??0);
            dto.Prices[DTOs.PizzaSize.Medium] = CalculatePrice(pizza, DTOs.PizzaSize.Medium, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "medium")?.Price ?? 0);
            dto.Prices[DTOs.PizzaSize.Big] = CalculatePrice(pizza, DTOs.PizzaSize.Big, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "big")?.Price ?? 0);

            dto.Weights[DTOs.PizzaSize.Small] = CalculateWeight(pizza, DTOs.PizzaSize.Small, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "small")?.Weight ?? 0);
            dto.Weights[DTOs.PizzaSize.Medium] = CalculateWeight(pizza, DTOs.PizzaSize.Medium, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "medium")?.Weight ?? 0);
            dto.Weights[DTOs.PizzaSize.Big] = CalculateWeight(pizza, DTOs.PizzaSize.Big, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "big")?.Weight ?? 0);


            return dto;
        }

        private IngredientDto MapToIngredientDto(Ingredient ingredient)
        {
            return new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Description = ingredient.Description,
                Small = ingredient.Small,
                Medium = ingredient.Medium,
                Big = ingredient.Big,
                PricePerGram = ingredient.PricePerGram
            };
        }

        public async Task<PizzaDto?> GetPizzaByIdAsync(int id)
        {
            var pizza = await _pizzaRepository.GetPizzaByIdAsync(id);
            var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            return (pizza != null ? MapToPizzaDto(pizza, pizzaSizes) : null);
            //if (pizza == null) return null;
            //return new PizzaDto
            //{
            //    Id = pizza.Id,
            //    Name = pizza.Name,
            //    Description = pizza.Description,
            //    IsAvailable = pizza.IsAvailable,
            //    Image = pizza.Image,
            //    Ingredients = pizza.Ingredients.Select(t => new IngredientDto
            //    {
            //        Id = t.Id,
            //        Name = t.Name,
            //        Description = t.Description,
            //        Small = t.Small,
            //        Medium = t.Medium,
            //        Big = t.Big,
            //        PricePerGram = t.PricePerGram
            //    }).ToList()
            //};
        }

        public async Task<List<IngredientDto>> GetIngredientsForPizzaAsync(int pizzaId)
        {
            var ingrs = await _pizzaRepository.GetIngredientsByPizzaIdAsync(pizzaId);

            return ingrs.Select(MapToIngredientDto).ToList();

            //if (ingrs == null || ingrs.Count == 0)
            //    return null;

            //return ingrs.Select(t => new IngredientDto
            //{
            //    Id = t.Id,
            //    Name = t.Name,
            //    Description = t.Description,
            //    Small = t.Small,
            //    Medium = t.Medium,
            //    Big = t.Big,
            //    PricePerGram = t.PricePerGram
            //}).ToList();
        }

        private async Task<List<Ingredient>> LoadIngredientsAsync(IEnumerable<int> ingredientIds)
        {
            return (await _ingrRepository.GetIngredientsByIdsAsync(ingredientIds)).ToList();
        }


        public async Task<PizzaDto> AddPizzaAsync(CreatePizzaDto pizzaDto)
        {

            if (pizzaDto == null)
                throw new ArgumentNullException(nameof(pizzaDto));

            if (string.IsNullOrWhiteSpace(pizzaDto.Name))
                throw new ArgumentException("Название пиццы обязательно");

            try
            {
                var ingredients = await LoadIngredientsAsync(pizzaDto.DefaultIngredientIds);

                if (ingredients.Count != pizzaDto.DefaultIngredientIds.Count())
                    throw new ArgumentException("Некоторые ингредиенты не найдены");


                var pizza = new Pizza
                {
                    Id = 0,
                    Name = pizzaDto.Name,
                    Description = pizzaDto.Description,
                    IsAvailable = pizzaDto.IsAvailable,
                    Image = pizzaDto.Image,
                    Ingredients = ingredients,
                    IsDeleted = false
                };
                //List<Ingredient> addedIngredients = new List<Ingredient>();
                //foreach (int a in pizzaDto.Ingredients)
                //{
                //    addedIngredients.Add(await _ingrRepository.GetIngredientByIdAsync(a));
                //}
                //pizza.Ingredients = addedIngredients;
                await _pizzaRepository.AddPizzaAsync(pizza);
                var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

                return MapToPizzaDto(pizza, pizzaSizes);
                //pizzaDto.Id = pizza.Id;
            }
            catch(Exception ex)
            {
                throw new ApplicationException($"Ошибка при добавлении пиццы: {ex.Message}", ex);
            }
        }

        public async Task<PizzaDto> UpdatePizzaAsync(UpdatePizzaDto pizzaDto)
        {
            var pizza = await _pizzaRepository.GetPizzaByIdAsync(pizzaDto.Id);

            if (pizza == null)
                throw new KeyNotFoundException("Пицца не найдена");

            var ingredients = await LoadIngredientsAsync(pizzaDto.Ingredients);


            pizza.Name = pizzaDto.Name;
            pizza.Description = pizzaDto.Description;
            pizza.Image = pizzaDto.Image;
            pizza.IsAvailable = pizzaDto.IsAvailable;

            // Обновление ингредиентов
            pizza.Ingredients = ingredients;

            await _pizzaRepository.UpdatePizzaAsync(pizza);
            var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            return MapToPizzaDto(pizza, pizzaSizes);
            //if (pizza != null)
            //{
            //    pizza.Name = pizzaDto.Name;
            //    pizza.Description = pizzaDto.Description;
            //    pizza.Image = pizzaDto.Image;
            //    pizza.IsAvailable = pizzaDto.IsAvailable;
            //    //ingredients
            //    await _pizzaRepository.UpdatePizzaAsync(pizza);
            //}
        }

        public async Task DeletePizzaAsync(int id)
        {
            try
            {
                await _pizzaRepository.DeletePizzaAsync(id);
            }
            catch (InvalidOperationException ex)
            {
               
                throw new ApplicationException($"Ошибка при удалении пиццы: {ex.Message}", ex);
            }
        }

        public async Task RestorePizzaAsync(int id)
        {
            try
            {
                await _pizzaRepository.RestorePizzaAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка восстановления пиццы", ex);
            }
        }

    }
}
