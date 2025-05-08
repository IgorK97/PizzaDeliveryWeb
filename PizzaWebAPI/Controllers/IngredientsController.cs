using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaWebAPI.Controllers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        private readonly IngredientService _ingredientService;
        private readonly ILogger<IngredientsController> _logger;
        private readonly IWebHostEnvironment _env;


        public IngredientsController(IngredientService ingredientService, IWebHostEnvironment env, ILogger<IngredientsController> logger)
        {
            _ingredientService = ingredientService;
            _logger = logger;
            _env = env;
        }
        
        
        // GET: api/<IngredientsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientDto>>> GetIngredientsAsync()
        {


            try
            {
                _logger.LogInformation("Запрос списка ингредиентов");
                var ingredients = await _ingredientService.GetIngredientsAsync();
                //var hasMore = pizzas.Any() && pizzas.Last().Id > lastId;
                //return Ok(new
                //{
                //    Items = pizzas,
                //    LastId = pizzas.LastOrDefault()?.Id ?? lastId,
                //    HasMore = hasMore
                //});
                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ингредиентов");
                return StatusCode(500, "Ошибка сервера: ошибка при получении ингредиентов :-(");
            }





            //var ingredients = await _ingredientService.GetIngredientsAsync();
            //return ingredients.Select(i => new IngredientDto
            //{
            //    Id = i.Id,
            //    Name = i.Name,
            //    Description = i.Description,
            //    Small = i.Small,
            //    Medium = i.Medium,
            //    Big = i.Big,
            //    PricePerGram = i.PricePerGram
            //});
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]

        public async Task<ActionResult<IngredientDto>> CreateIngredient([FromBody] CreateIngredientDto ingredientDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                string imageUrl = await ProcessImageAsync(ingredientDto.Image);




                ingredientDto.Image = imageUrl;
                //pizzaDto.Id = 0;
                var createdIngredient = await _ingredientService.AddIngredientAsync(ingredientDto);
                return CreatedAtAction(nameof(GetIngredientById),
                    new { id = createdIngredient.Id }, createdIngredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания пиццы");
                return BadRequest(ex.Message);
            }


            //string? imagePath = null;
            //if (ingredientDto.Image != null)
            //{
            //    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ingredientDto.Image.FileName);
            //    var path = Path.Combine("wwwroot", "images", "pizzas", fileName);
            //    using (var stream = new FileStream(path, FileMode.Create))
            //    {
            //        await ingredientDto.Image.CopyToAsync(stream);
            //    }
            //    imagePath = $"/images/pizzas/{fileName}";
            //}
            //IngredientDto iDto = new IngredientDto
            //{
            //    Id = 0,
            //    Name = ingredientDto.Name,
            //    Description = ingredientDto.Description,
            //    IsAvailable = ingredientDto.IsAvailable/*=="true"?true:false*/,
            //    Image = imagePath,
            //    Big=ingredientDto.Big,
            //    Medium=ingredientDto.Medium,
            //    PricePerGram=ingredientDto.PricePerGram,
            //    Small=ingredientDto.Small
            //};
            ////pizzaDto.ImageUrl = imagePath;
            //await _ingredientService.AddIngredientAsync(iDto);
            //return CreatedAtAction(nameof(GetIngredientById),
            //    new { id = iDto.Id }, iDto);

            //await _ingredientService.AddIngredientAsync(ingrDto);
            //return CreatedAtAction(nameof(GetIngredientById),
            //    new { id = iDto.Id }, iDto);
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

        private async Task<string> ProcessImageAsync(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image))
                return string.Empty;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "ingredients");

            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(uploadsFolder, fileName);

            var imageData = base64Image.Split(',')[1];
            await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(imageData));

            return $"/images/ingredients/{fileName}";
        }

        // PUT api/<IngredientsController>/5
        //[Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]

        public async Task<IActionResult> UpdateIngredient(int id, UpdateIngredientDto ingrDto)
        {

            try
            {
                if (id != ingrDto.Id)
                    return BadRequest();
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var existingIngredient = await _ingredientService.GetIngredientByIdAsync(id);
                if (existingIngredient == null) return NotFound();

                string imageUrl = existingIngredient.Image;
                if (!string.IsNullOrEmpty(ingrDto.Image))
                {
                    DeleteOldImage(existingIngredient.Image);
                    imageUrl = await ProcessImageAsync(ingrDto.Image);
                }

                ingrDto.Image = imageUrl;

                //pizzaDto.Image = imageUrl;

                //var pizzaToUpdate = new UpdateIngredientDto
                //{
                //    Id = id,
                //    Name = ingrDto.Name,
                //    Description = ingrDto.Description,
                //    IsAvailable = pizzaDto.IsAvailable,
                //    Ingredients = pizzaDto.Ingredients,
                //    Image = imageUrl
                //};

                var updatedIngr = await _ingredientService.UpdateIngredientAsync(ingrDto);
                return Ok(updatedIngr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления пиццы {PizzaId}", id);
                return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
            }
            //if (id != ingrDto.Id) return BadRequest();
            //await _ingredientService.UpdateIngredientAsync(ingrDto);
            //return NoContent();
        }

        // DELETE api/<IngredientsController>/5
        //[Authorize(Roles ="manager")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            try
            {
                await _ingredientService.DeleteIngredientAsync(id);
                return NoContent();
            }
            catch(ApplicationException ex)
            {
                return StatusCode(500, "Не удалось удалить ингредиент");
            }
        }
    }
}
