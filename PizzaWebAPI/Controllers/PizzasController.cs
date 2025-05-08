using System.Linq.Expressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.API.Models;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Infrastructure.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaWebAPI.Controllers
{
    /// <summary>
    /// Контроллер для работы с пиццами.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PizzasController : ControllerBase
    {
        private readonly PizzaService _pizzaService;
        private readonly ILogger<PizzasController> _logger;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Конструктор контроллера.
        /// </summary>
        /// <param name="logger">Логгер для контроллера.</param>
        /// <param name="env">Среда веб-хоста для получения информации о хосте.</param>
        /// <param name="pizzaService">Сервис для работы с пиццами.</param>
        public PizzasController(ILogger<PizzasController> logger, IWebHostEnvironment env, PizzaService pizzaService)
        {
            _pizzaService = pizzaService;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Получение списка пицц с пагинацией.
        /// </summary>
        /// <param name="lastId">Идентификатор последней пиццы на предыдущей странице.</param>
        /// <param name="pageSize">Количество пицц на одной странице.</param>
        /// <returns>Список пицц с информацией о наличии следующих элементов.</returns>
        // GET: api/<PizzasController>
        [HttpGet]
        public async Task<ActionResult<ResultGetPizzas>> GetPizzas(
            [FromQuery] int lastId = 0,
            [FromQuery] int pageSize = 10
            )
        {
            try
            {
                _logger.LogInformation("Запрос списка пицц, {lastId}, {pageSize}", lastId, pageSize);
                var pizzas = await _pizzaService.GetPizzasAsync(lastId, pageSize, true);
                var hasMore = pizzas.Any() && pizzas.Last().Id > lastId;
                return Ok(new ResultGetPizzas
                {
                    Items = pizzas,
                    LastId = pizzas.LastOrDefault()?.Id ?? lastId,
                    HasMore = hasMore
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пицц при запросе {lastId}, {pageSize}", lastId, pageSize);
                return StatusCode(500, "Ошибка сервера: ошибка при получении пицц :-(");
            }
        }


        /// <summary>
        /// Получение пиццы по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пиццы.</param>
        /// <returns>Пицца с указанным идентификатором.</returns>
        // GET api/<PizzasController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PizzaDto>> GetPizzaById(int id)
        {
            var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            return pizza != null ? Ok(pizza) : NotFound();
           
        }

        /// <summary>
        /// Получение списка ингредиентов для пиццы.
        /// </summary>
        /// <param name="id">Идентификатор пиццы.</param>
        /// <returns>Список ингредиентов для пиццы.</returns>
        [HttpGet("{id}/ingredients")]
        public async Task<ActionResult<IEnumerable<IngredientDto>>>
            GetIngredientsForPizza(int id)
        {
            var ingredients = await _pizzaService.GetIngredientsForPizzaAsync(id);
            if (ingredients == null || !ingredients.Any())
                return NotFound($"Не найдено ингредиентов для такой пиццы с ID {id}.");
            return Ok(ingredients);
        }

        /// <summary>
        /// Создание новой пиццы.
        /// </summary>
        /// <param name="pizzaDto">Данные для создания пиццы.</param>
        /// <returns>Созданная пицца.</returns>
        // POST api/<PizzasController>
        //[Authorize(Roles = "manager")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]
        [HttpPost]
        public async Task<ActionResult<PizzaDto>> CreatePizza( [FromBody]CreatePizzaDto pizzaDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                string imageUrl = await ProcessImageAsync(pizzaDto.Image);

               


                pizzaDto.Image = imageUrl;
                //pizzaDto.Id = 0;
                var createdPizza = await _pizzaService.AddPizzaAsync(pizzaDto);
                return CreatedAtAction(nameof(GetPizzaById),
                    new { id = createdPizza.Id }, createdPizza);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания пиццы");
                return BadRequest(ex.Message);
            }

        }


        /// <summary>
        /// Обработка изображения в формате base64.
        /// </summary>
        /// <param name="base64Image">Изображение в формате base64.</param>
        /// <returns>URL обработанного изображения.</returns>
        private async Task<string> ProcessImageAsync(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image))
                return string.Empty;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "pizzas");

            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(uploadsFolder, fileName);

            var imageData = base64Image.Split(',')[1];
            await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(imageData));

            return $"/images/pizzas/{fileName}";
        }


        /// <summary>
        /// Удаление старого изображения.
        /// </summary>
        /// <param name="imagePath">Путь к изображению, которое нужно удалить.</param>
        private void DeleteOldImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        /// <summary>
        /// Обновление данных пиццы.
        /// </summary>
        /// <param name="id">Идентификатор пиццы для обновления.</param>
        /// <param name="pizzaDto">Данные для обновления пиццы.</param>
        /// <returns>Обновленная пицца.</returns>
        // PUT api/<PizzasController>/5
        //[Authorize(Roles ="manager")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePizza(int id, /*[FromForm]*/ UpdatePizzaDto pizzaDto)
        {
            try
            {
                if (id != pizzaDto.Id)
                return BadRequest("ID в пути и теле запроса не совпадают");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            
                var existingPizza = await _pizzaService.GetPizzaByIdAsync(id);
                if (existingPizza == null) return NotFound();

                string imageUrl = existingPizza.Image;
                if (!string.IsNullOrEmpty(pizzaDto.Image))
                {
                    DeleteOldImage(existingPizza.Image);
                    imageUrl = await ProcessImageAsync(pizzaDto.Image);
                }

                //pizzaDto.Image = imageUrl;

                var pizzaToUpdate = new UpdatePizzaDto
                {
                    Id = id,
                    Name = pizzaDto.Name,
                    Description = pizzaDto.Description,
                    IsAvailable = pizzaDto.IsAvailable,
                    Ingredients = pizzaDto.Ingredients,
                    Image = imageUrl
                };

                var updatedPizza = await _pizzaService.UpdatePizzaAsync(pizzaToUpdate);
                _logger.LogInformation("Обновление пиццы {PizzaId}", updatedPizza.Id);

                return Ok(updatedPizza);
            }
            
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления пиццы {PizzaId}", id);
                return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
            }

        }

        /// <summary>
        /// Удаление пиццы.
        /// </summary>
        /// <param name="id">Идентификатор пиццы для удаления.</param>
        /// <returns>Результат операции удаления.</returns>
        // DELETE api/<PizzasController>/5
        //[Authorize(Roles = "manager")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePizza(int id)
        {

            try
            {
                await _pizzaService.DeletePizzaAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления пиццы {PizzaId}", id);
                return StatusCode(500, "Не удалось удалить пиццу");
            }


            //var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            //if(pizza == null)
            //{
            //    return NotFound();
            //}
            //if (!string.IsNullOrEmpty(pizza.Image))
            //{
            //    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pizza.Image.TrimStart('/'));
            //    if (System.IO.File.Exists(imagePath))
            //    {
            //        System.IO.File.Delete(imagePath);
            //    }
            //}
            //await _pizzaService.DeletePizzaAsync(id);
            //return NoContent();



            //try
            //{
            //    await _pizzaService.DeletePizzaAsync(id);
            //    return NoContent();
            //}
            //catch(ApplicationException ex)
            //{
            //    return Conflict(new { message = ex.Message });
            //}
        }


        /// <summary>
        /// Восстановление удаленной пиццы.
        /// </summary>
        /// <param name="id">Идентификатор пиццы для восстановления.</param>
        /// <returns>Результат восстановления пиццы.</returns>
        [HttpPatch("{id}/restore")]
        //[Authorize(Roles = "manager")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]

        public async Task<IActionResult> RestorePizza(int id)
        {
            try
            {
                await _pizzaService.RestorePizzaAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка восстановления пиццы {PizzaId}", id);
                return StatusCode(500, "Не удалось восстановить пиццу");
            }
        }


        /// <summary>
        /// Получение пицц для управления (доступные и недоступные).
        /// </summary>
        /// <param name="lastId">Идентификатор последней пиццы на предыдущей странице.</param>
        /// <param name="pageSize">Количество пицц на одной странице.</param>
        /// <returns>Список доступных и недоступных пицц.</returns>
        [HttpGet("manage")]
        //[Authorize(Roles = "Manager")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]

        public async Task<IActionResult> GetPizzasForManagement(
        [FromQuery] int lastId = 0,
        [FromQuery] int pageSize = 10)
        {
            var pizzas = await _pizzaService.GetPizzasAsync(lastId, pageSize, true);
            return Ok(new
            {
                Available = pizzas.Where(p => p.IsAvailable),
                Unavailable = pizzas.Where(p => !p.IsAvailable)
            });
        }
    }
    
    }
