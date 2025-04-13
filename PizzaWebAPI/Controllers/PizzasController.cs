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
            //var baseUrl = $"{Request.Scheme}://{Request.Host}";
            //pizza.Image = pizza.Image != "" ? $"{baseUrl}{pizza.Image}" : "";
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
        public async Task<ActionResult<PizzaDto>> CreatePizza(/*[FromForm]*/ [FromBody]CreateNewPizzaDto pizzaDto)
        {
            
            string fileName;
            if (string.IsNullOrEmpty(pizzaDto.Image))
            {
                return BadRequest("Изображение не передано");
            }
            var imageBytes = Convert.FromBase64String(pizzaDto.Image.Split(',')[1]);
            var uploadsFolder = Path.Combine("wwwroot", "images", "pizzas");
            Directory.CreateDirectory(uploadsFolder);
            fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(uploadsFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
            CreatePizzaDto pDto = new CreatePizzaDto
            {
                Id = 0,
                Name = pizzaDto.Name,
                Description=pizzaDto.Description,
                IsAvailable=pizzaDto.IsAvailable/*=="true"?true:false*/,
                Image=$"/images/pizzas/{fileName}"
            };
            //pizzaDto.ImageUrl = imagePath;
            await _pizzaService.AddPizzaAsync(pDto);
            return CreatedAtAction(nameof(GetPizza),
                new { id = pDto.Id }, pDto);
        }

        // PUT api/<PizzasController>/5
        //[Authorize(Roles ="admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePizza(int id, /*[FromForm]*/ CreateNewPizzaDto pizzaDto)
        {
            var pizza = await _pizzaService.GetPizzaByIdAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            PizzaDto pDto = new PizzaDto
            {
                Id=id,
                Name = pizzaDto.Name,
                Description = pizzaDto.Description,
                IsAvailable = pizzaDto.IsAvailable,
                Image=pizza.Image
            };
            if (!string.IsNullOrEmpty(pizzaDto.Image))
            {
                
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pizza.Image.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                var uploadsFolder = Path.Combine("wwwroot","images","pizzas");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}.png";
                pDto.Image = $"/images/pizzas/{fileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                var imageBytes = Convert.FromBase64String(pizzaDto.Image.Split(',')[1]);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
               
                //pDto.Image = $"/images/pizzas/{fileName}";
            }
            //pDto.Image = pizza.Image;
            await _pizzaService.UpdatePizzaAsync(pDto);

            return Ok(pDto);
            //return CreatedAtAction(nameof(GetPizza), new { id = pizzaDto.Id }, pizzaDto);
            //return NoContent();

            //if (id != pizzaDto.Id) return BadRequest();
            //await _pizzaService.UpdatePizzaAsync(pizzaDto);
            //return NoContent();
        }


        // DELETE api/<PizzasController>/5
        //[Authorize(Roles = "admin")]
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
    }
}
