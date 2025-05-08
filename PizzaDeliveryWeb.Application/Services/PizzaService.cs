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
    /// <summary>
    /// Сервис для управления пиццами, включая получение, добавление, обновление и удаление.
    /// </summary>
    public class PizzaService
    {
        private readonly IPizzaRepository _pizzaRepository;
        private readonly IIngredientRepository _ingrRepository;
        private readonly IPizzaSizeRepository _pizzaSizeRepository;

        /// <summary>
        /// Конструктор сервиса пиццы.
        /// </summary>
        /// <param name="pizzaRepository">Репозиторий пицц</param>
        /// <param name="ingrRepository">Репозиторий ингредиентов</param>
        /// <param name="pizzaSizeRepository">Репозиторий размеров пицц</param>
        public PizzaService(IPizzaRepository pizzaRepository, IIngredientRepository ingrRepository, IPizzaSizeRepository pizzaSizeRepository)
        {
            _pizzaRepository = pizzaRepository;
            _ingrRepository = ingrRepository;
            _pizzaSizeRepository = pizzaSizeRepository;
        }


        /// <summary>
        /// Расчет цены пиццы с учетом ингредиентов и базовой цены.
        /// </summary>
        /// <param name="pizza">Пицца</param>
        /// <param name="ps">Размер пиццы</param>
        /// <param name="basePrice">Базовая цена</param>
        /// <returns>Общая цена пиццы</returns>
        private decimal CalculatePrice(Pizza pizza, DTOs.PizzaSizeEnum ps, decimal basePrice)
        {
            var ingrPrice = pizza.Ingredients.Sum(pi =>
            {
                var grams = ps switch
                {
                    DTOs.PizzaSizeEnum.Small => pi.Small,
                    DTOs.PizzaSizeEnum.Medium => pi.Medium,
                    DTOs.PizzaSizeEnum.Big => pi.Big,
                    _ => throw new InvalidOperationException("Неизвестный размер пиццы")
                };
                return grams * pi.PricePerGram;
            });
            decimal totalPrice = ingrPrice + basePrice;
            return totalPrice;
        }


        /// <summary>
        /// Расчет общего веса пиццы на основе ингредиентов и базового веса.
        /// </summary>
        /// <param name="pizza">Пицца</param>
        /// <param name="ps">Размер пиццы</param>
        /// <param name="baseWeight">Базовый вес</param>
        /// <returns>Общий вес пиццы</returns>
        private decimal CalculateWeight(Pizza pizza, DTOs.PizzaSizeEnum ps, decimal baseWeight)
        {
            var ingrWeight = pizza.Ingredients.Sum(pi =>
            {
                var grams = ps switch
                {
                    DTOs.PizzaSizeEnum.Small => pi.Small,
                    DTOs.PizzaSizeEnum.Medium => pi.Medium,
                    DTOs.PizzaSizeEnum.Big => pi.Big,
                    _ => throw new InvalidOperationException("Неизвестный размер пиццы")
                };
                return grams;
            });
            decimal totalWeight = ingrWeight + baseWeight;
            return totalWeight;
        }


        /// <summary>
        /// Получает список доступных пицц с учетом пагинации и фильтрации.
        /// </summary>
        /// <param name="lastId">Последний ID для пагинации</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <param name="includeUnavailable">Включать ли недоступные пиццы</param>
        /// <returns>Список DTO пицц</returns>
        public async Task<IEnumerable<PizzaDto>> GetPizzasAsync(int lastId = 0, 
            int pageSize=10,
            bool includeUnavailable = false)
        {
            var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            var pizzas = await _pizzaRepository.GetPizzasAsync(lastId, pageSize,
                includeUnavailable? null: true);
            return pizzas.Select(p => MapToPizzaDto(p, pizzaSizes));
            
        }


        /// <summary>
        /// Преобразует объект пиццы в DTO.
        /// </summary>
        /// <param name="pizza">Пицца</param>
        /// <param name="pizzaSizes">Список размеров пицц</param>
        /// <returns>Объект PizzaDto</returns>
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
                Prices = new Dictionary<int, decimal>(),
                Weights = new Dictionary<int, decimal>()
            };

            //var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            //decimal smallPrice = pizzaSizes.FirstOrDefault(p => p.Id == 1).Price;

            dto.Prices[(int)DTOs.PizzaSizeEnum.Small] = CalculatePrice(pizza, DTOs.PizzaSizeEnum.Small, pizzaSizes.FirstOrDefault(p=>p.Name.ToLower()=="small")?.Price??0);
            dto.Prices[(int)DTOs.PizzaSizeEnum.Medium] = CalculatePrice(pizza, DTOs.PizzaSizeEnum.Medium, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "medium")?.Price ?? 0);
            dto.Prices[(int)DTOs.PizzaSizeEnum.Big] = CalculatePrice(pizza, DTOs.PizzaSizeEnum.Big, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "big")?.Price ?? 0);

            dto.Weights[(int)DTOs.PizzaSizeEnum.Small] = CalculateWeight(pizza, DTOs.PizzaSizeEnum.Small, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "small")?.Weight ?? 0);
            dto.Weights[(int)DTOs.PizzaSizeEnum.Medium] = CalculateWeight(pizza, DTOs.PizzaSizeEnum.Medium, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "medium")?.Weight ?? 0);
            dto.Weights[(int)DTOs.PizzaSizeEnum.Big] = CalculateWeight(pizza, DTOs.PizzaSizeEnum.Big, pizzaSizes.FirstOrDefault(p => p.Name.ToLower() == "big")?.Weight ?? 0);


            return dto;
        }


        /// <summary>
        /// Преобразует объект ингредиента в DTO.
        /// </summary>
        /// <param name="ingredient">Ингредиент</param>
        /// <returns>Объект IngredientDto</returns>
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


        /// <summary>
        /// Получает пиццу по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пиццы</param>
        /// <returns>DTO пиццы или null</returns>
        public async Task<PizzaDto?> GetPizzaByIdAsync(int id)
        {
            var pizza = await _pizzaRepository.GetPizzaByIdAsync(id);
            var pizzaSizes = await _pizzaSizeRepository.GetPizzaSizesAsync();

            return (pizza != null ? MapToPizzaDto(pizza, pizzaSizes) : null);

     
        }


        /// <summary>
        /// Получает ингредиенты, связанные с конкретной пиццей.
        /// </summary>
        /// <param name="pizzaId">ID пиццы</param>
        /// <returns>Список DTO ингредиентов</returns>
        public async Task<List<IngredientDto>> GetIngredientsForPizzaAsync(int pizzaId)
        {
            var ingrs = await _pizzaRepository.GetIngredientsByPizzaIdAsync(pizzaId);

            return ingrs.Select(MapToIngredientDto).ToList();

        }


        /// <summary>
        /// Загружает список ингредиентов по их идентификаторам.
        /// </summary>
        /// <param name="ingredientIds">Список ID ингредиентов</param>
        /// <returns>Список объектов ингредиентов</returns>
        private async Task<List<Ingredient>> LoadIngredientsAsync(IEnumerable<int> ingredientIds)
        {
            return (await _ingrRepository.GetIngredientsByIdsAsync(ingredientIds)).ToList();
        }

        /// <summary>
        /// Добавляет новую пиццу.
        /// </summary>
        /// <param name="pizzaDto">DTO новой пиццы</param>
        /// <returns>Добавленная пицца в виде DTO</returns>
        /// <exception cref="ArgumentNullException">Если DTO пиццы равен null</exception>
        /// <exception cref="ArgumentException">Если отсутствуют обязательные данные</exception>
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

        /// <summary>
        /// Обновляет существующую пиццу.
        /// </summary>
        /// <param name="pizzaDto">DTO пиццы для обновления</param>
        /// <returns>Обновленная пицца в виде DTO</returns>
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

        }


        /// <summary>
        /// Удаляет пиццу по ID (логически)
        /// </summary>
        /// <param name="id">ID пиццы</param>
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


        /// <summary>
        /// Восстанавливает ранее удаленную пиццу.
        /// </summary>
        /// <param name="id">ID пиццы</param>
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
