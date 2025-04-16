using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Infrastructure.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzasController : ControllerBase
    {
        private readonly PizzaService _pizzaService;
        private readonly ILogger<PizzasController> _logger;
        private readonly IWebHostEnvironment _env;
        public PizzasController(ILogger<PizzasController> logger, IWebHostEnvironment env, PizzaService pizzaService)
        {
            _pizzaService = pizzaService;
            _logger = logger;
            _env = env;
        }
        // GET: api/<PizzasController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PizzaDto>>> GetPizzas(
            [FromQuery] int lastId = 0,
            [FromQuery] int pageSize = 10
            )
        {
            try
            {
                _logger.LogInformation("Запрос списка пицц, {lastId}, {pageSize}", lastId, pageSize);
                var pizzas = await _pizzaService.GetPizzasAsync(lastId, pageSize, true);
                var hasMore = pizzas.Any() && pizzas.Last().Id > lastId;
                return Ok(new
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

        // GET api/<PizzasController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PizzaDto>> GetPizzaById(int id)
        {
            var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            return pizza != null ? Ok(pizza) : NotFound();
            //if (pizza == null)
            //{
            //    return NotFound();
            //}
            ////var baseUrl = $"{Request.Scheme}://{Request.Host}";
            ////pizza.Image = pizza.Image != "" ? $"{baseUrl}{pizza.Image}" : "";
            //return Ok(pizza);
        }
        [HttpGet("{id}/ingredients")]
        public async Task<ActionResult<IEnumerable<IngredientDto>>>
            GetIngredientsForPizza(int id)
        {
            var ingredients = await _pizzaService.GetIngredientsForPizzaAsync(id);
            if (ingredients == null || !ingredients.Any())
                return NotFound($"Не найдено ингредиентов для такой пиццы с ID {id}.");
            return Ok(ingredients);
        }
        // POST api/<PizzasController>
        [Authorize(Roles = "manager")]
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

            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);


            //try
            //{
            //    await _pizzaService.AddPizzaAsync(pizzaDto);
            //    return CreatedAtAction(
            //        nameof(GetPizzaById),
            //        new { id = pizzaDto.Id }, pizzaDto);
            //}
            //catch(Exception ex)
            //{
            //    _logger.LogError(ex, "Ошибка при получении пицц при создании пиццы");

            //    return BadRequest(ex.Message);
            //}
            
            //string fileName;
            //if (string.IsNullOrEmpty(pizzaDto.Image))
            //{
            //    return BadRequest("Изображение не передано");
            //}
            //var imageBytes = Convert.FromBase64String(pizzaDto.Image.Split(',')[1]);
            //var uploadsFolder = Path.Combine("wwwroot", "images", "pizzas");
            //Directory.CreateDirectory(uploadsFolder);
            //fileName = $"{Guid.NewGuid()}.png";
            //var filePath = Path.Combine(uploadsFolder, fileName);
            //await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
            //CreatePizzaDto pDto = new CreatePizzaDto
            //{
            //    Id = 0,
            //    Name = pizzaDto.Name,
            //    Description=pizzaDto.Description,
            //    IsAvailable=pizzaDto.IsAvailable/*=="true"?true:false*/,
            //    Image=$"/images/pizzas/{fileName}"
            //};
            ////pizzaDto.ImageUrl = imagePath;
            //await _pizzaService.AddPizzaAsync(pDto);
            //return CreatedAtAction(nameof(GetPizza),
            //    new { id = pDto.Id }, pDto);
        }

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

        private void DeleteOldImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // PUT api/<PizzasController>/5
        [Authorize(Roles ="manager")]
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
                return Ok(updatedPizza);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления пиццы {PizzaId}", id);
                return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
            }





            //if (id != pizzaDto.Id)
            //    return BadRequest("ID в пути и теле запроса не совпадают");

            //try
            //{
            //    await _pizzaService.UpdatePizzaAsync(pizzaDto);
            //}







            //var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            //if (pizza == null)
            //{
            //    return NotFound();
            //}
            //PizzaDto pDto = new PizzaDto
            //{
            //    Id=id,
            //    Name = pizzaDto.Name,
            //    Description = pizzaDto.Description,
            //    IsAvailable = pizzaDto.IsAvailable,
            //    Image=pizza.Image
            //};
            //if (!string.IsNullOrEmpty(pizzaDto.Image))
            //{
                
            //    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pizza.Image.TrimStart('/'));
            //    if (System.IO.File.Exists(oldImagePath))
            //    {
            //        System.IO.File.Delete(oldImagePath);
            //    }
            //    var uploadsFolder = Path.Combine("wwwroot","images","pizzas");
            //    Directory.CreateDirectory(uploadsFolder);

            //    var fileName = $"{Guid.NewGuid()}.png";
            //    pDto.Image = $"/images/pizzas/{fileName}";
            //    var filePath = Path.Combine(uploadsFolder, fileName);

            //    var imageBytes = Convert.FromBase64String(pizzaDto.Image.Split(',')[1]);
            //    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
               
            //    //pDto.Image = $"/images/pizzas/{fileName}";
            //}
            ////pDto.Image = pizza.Image;
            //await _pizzaService.UpdatePizzaAsync(pDto);

            //return Ok(pDto);




            //return CreatedAtAction(nameof(GetPizza), new { id = pizzaDto.Id }, pizzaDto);
            //return NoContent();

            //if (id != pizzaDto.Id) return BadRequest();
            //await _pizzaService.UpdatePizzaAsync(pizzaDto);
            //return NoContent();
        }


        // DELETE api/<PizzasController>/5
        [Authorize(Roles = "manager")]
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

        [HttpPatch("{id}/restore")]
        [Authorize(Roles = "manager")]
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

        [HttpGet("manage")]
        [Authorize(Roles = "Manager")]
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
