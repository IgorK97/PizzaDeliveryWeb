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
        private readonly IWebHostEnvironment _env;
        public PizzasController(IWebHostEnvironment env, PizzaService pizzaService)
        {
            _pizzaService = pizzaService;
            _env = env;
        }
        // GET: api/<PizzasController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PizzaDto>>> GetPizzas()
        {
            var pizzas = await _pizzaService.GetPizzasAsync();
            return Ok(pizzas);
        }

        // GET api/<PizzasController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PizzaDto>> GetPizza(int id)
        {
            var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            pizza.Image = pizza.Image != "" ? $"{baseUrl}{pizza.Image}" : "";
            return Ok(pizza);
        }
        [HttpGet("{id}/ingredients")]
        public async Task<ActionResult<IEnumerable<IngredientDto>>>
            GetIngredientsForPizza(int id)
        {
            var ingredients = await _pizzaService.GetIngredintsForPizzaAsync(id);
            if (ingredients == null || !ingredients.Any())
                return NotFound($"Не найдено ингредиентов для такой пиццы с ID {id}.");
            return Ok(ingredients);
        }
        // POST api/<PizzasController>
        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<PizzaDto>> CreatePizza([FromForm]CreateNewPizzaDto pizzaDto)
        {
            string? imagePath = null;
            if (pizzaDto.Image != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(pizzaDto.Image.FileName);
                var path = Path.Combine("wwwroot", "images", "pizzas", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await pizzaDto.Image.CopyToAsync(stream);
                }
                imagePath = $"/images/pizzas/{fileName}";
            }
            CreatePizzaDto pDto = new CreatePizzaDto
            {
                Id = 0,
                Name = pizzaDto.Name,
                Description=pizzaDto.Description,
                IsAvailable=pizzaDto.IsAvailable/*=="true"?true:false*/,
                Image=imagePath
            };
            //pizzaDto.ImageUrl = imagePath;
            await _pizzaService.AddPizzaAsync(pDto);
            return CreatedAtAction(nameof(GetPizza),
                new { id = pDto.Id }, pDto);
        }

        // PUT api/<PizzasController>/5
        //[Authorize(Roles ="admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePizza(int id, [FromForm] CreatePizzaDto pizzaDto, IFormFile? file)
        {
            var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            if (file != null)
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pizza.Image.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var newImagePath = Path.Combine("wwwroot", "images", "pizzas", fileName);
                using (var stream = new FileStream(newImagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                pizza.Image = $"/images/pizzas/{fileName}";
            }
            pizzaDto.Image = pizza.Image;
            await _pizzaService.UpdatePizzaAsync(pizzaDto);
            return NoContent();
            //if (id != pizzaDto.Id) return BadRequest();
            //await _pizzaService.UpdatePizzaAsync(pizzaDto);
            //return NoContent();
        }


        // DELETE api/<PizzasController>/5
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePizza(int id)
        {
            var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            if(pizza == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(pizza.Image))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pizza.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            await _pizzaService.DeletePizzaAsync(id);
            return NoContent();
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
        //[Authorize (Roles = "admin")]
        //[HttpGet("pizzas/{id}/image")]
        //public async Task<ActionResult<PizzaDto>> GetPizzaImage(int id)
        //{
        //    var pizza = await _pizzaService.GetPizzaByIdAsync(id);

        //    if (pizza == null || string.IsNullOrEmpty(pizza.Image))
        //        return NotFound();

        //    // Локальный файл
        //    var filePath = Path.Combine(_env.WebRootPath, pizza.Image.TrimStart('/'));
        //    if (!System.IO.File.Exists(filePath))
        //        return NotFound();

        //    return PhysicalFile(filePath, "image/jpeg"); // или другой MIME-тип
        //}
        //[Authorize (Roles = "admin")]
        //[HttpPost("pizzas/{id}/upload-image")]
        //public async Task<IActionResult> UploadImage(int id, IFormFile file)
        //{
        //    // Валидация файла
        //    if (file == null || file.Length == 0)
        //        return BadRequest("Файл не выбран.");

        //    if (file.Length > 5 * 1024 * 1024) // Ограничение: 5 МБ
        //        return BadRequest("Файл слишком большой.");

        //    var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
        //    if (!allowedMimeTypes.Contains(file.ContentType))
        //        return BadRequest("Недопустимый формат файла.");

        //    // Поиск пиццы
        //    var pizza = await _pizzaService.GetPizzaByIdAsync(id);
        //    if (pizza == null)
        //        return NotFound("Пицца не найдена.");

        //    // Генерация уникального имени файла
        //    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        //    var folderPath = Path.Combine(_env.WebRootPath, "images/pizzas");
        //    Directory.CreateDirectory(folderPath); // Создать папку, если её нет
        //    var filePath = Path.Combine(folderPath, fileName);

        //    // Сохранение файла
        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    // Обновление записи в БД
        //    pizza.Image = $"/images/pizzas/{fileName}";
        //    List<int> ingrs = new List<int>();
        //    foreach (IngredientDto i in pizza.Ingredients)
        //        ingrs.Add(i.Id);
        //    CreatePizzaDto updatedPizza = new CreatePizzaDto
        //    {
        //        Id=pizza.Id,
        //        Name = pizza.Name,
        //        Description = pizza.Description,
        //        Image = pizza.Image,
        //        Ingredients = ingrs
        //    };
        //    await _pizzaService.UpdatePizzaAsync(updatedPizza);

        //    return Ok(new { ImageUrl = pizza.Image });
        //}
        //[Authorize(Roles = "admin")]
        //[HttpDelete("pizzas/{id}/image")]
        //public async Task<IActionResult> DeleteImage(int id)
        //{
        //    var pizza = await _pizzaService.GetPizzaByIdAsync(id);
        //    if (pizza == null || string.IsNullOrEmpty(pizza.Image))
        //        return NotFound();

        //    var filePath = Path.Combine(_env.WebRootPath, pizza.Image.TrimStart('/'));
        //    if (System.IO.File.Exists(filePath))
        //        System.IO.File.Delete(filePath);

        //    pizza.Image = "";
        //    List<int> ingrs = new List<int>();
        //    foreach (IngredientDto i in pizza.Ingredients)
        //        ingrs.Add(i.Id);
        //    CreatePizzaDto updatedPizza = new CreatePizzaDto
        //    {
        //        Id = pizza.Id,
        //        Name = pizza.Name,
        //        Description = pizza.Description,
        //        Image = pizza.Image,
        //        Ingredients = ingrs
        //    };
        //    await _pizzaService.UpdatePizzaAsync(updatedPizza);

        //    return NoContent();
        //}
    }
}
