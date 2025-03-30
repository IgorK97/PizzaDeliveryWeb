using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        private readonly IngredientService _ingredientService;
        
        
        public IngredientsController(IngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }
        
        
        // GET: api/<IngredientsController>
        [HttpGet]
        public async Task<IEnumerable<IngredientDto>> GetIngredientsAsync()
        {
            var ingredients = await _ingredientService.GetIngredientsAsync();
            return ingredients.Select(i => new IngredientDto
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

        // GET api/<IngredientsController>/5
        [HttpGet("{id}")]
        public async Task<IngredientDto> GetIngredientById(int id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
            if (ingredient == null) return null;
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

        // POST api/<IngredientsController>
        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<IngredientDto>> CreateIngredient(IngredientDto ingrDto)
        {
            await _ingredientService.AddIngredientAsync(ingrDto);
            return CreatedAtAction(nameof(GetIngredientById),
                new { id = ingrDto.Id }, ingrDto);
        }

        // PUT api/<IngredientsController>/5
        //[Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, IngredientDto ingrDto)
        {
            if (id != ingrDto.Id) return BadRequest();
            await _ingredientService.UpdateIngredientAsync(ingrDto);
            return NoContent();
        }

        // DELETE api/<IngredientsController>/5
        [Authorize(Roles ="admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _ingredientService.DeleteIngredientAsync(id);
                return NoContent();
            }
            catch(ApplicationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
