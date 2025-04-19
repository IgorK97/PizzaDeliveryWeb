using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly CartService _cartService;
        private readonly ILogger<CartsController> _logger;
        public CartItemsController(UserManager<User> userManager, CartService cartService, ILogger<CartsController> logger)
        {
            _userManager = userManager;
            _cartService = cartService;
            _logger = logger;
        }

       

        // POST api/<CartItemsController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<CartItemsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<CartDto>> Put(int id, [FromBody]NewCartItemDto itemDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var cart = await _cartService.UpdateItemFromcartAsync(itemDto);
            return Ok(cart);
        }

        // DELETE api/<CartItemsController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles="client")]
        public async Task<ActionResult<CartDto>> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var cart = await _cartService.RemoveItemFromCartAsync(id);
            _logger.LogInformation("Удаление позиции корзины");
            return Ok(cart);
        }
    }
}
