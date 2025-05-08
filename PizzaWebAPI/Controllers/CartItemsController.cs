using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    /// <summary>
    /// Контроллер для управления позициями корзины пользователя (обновление и удаление).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly CartService _cartService;
        private readonly ILogger<CartsController> _logger;

        /// <summary>
        /// Конструктор контроллера CartItems.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей.</param>
        /// <param name="cartService">Сервис корзины.</param>
        /// <param name="logger">Логгер для записи событий.</param>
        public CartItemsController(UserManager<User> userManager, CartService cartService, ILogger<CartsController> logger)
        {
            _userManager = userManager;
            _cartService = cartService;
            _logger = logger;
        }


        /// <summary>
        /// Обновляет позицию в корзине.
        /// </summary>
        /// <param name="id">Идентификатор позиции корзины.</param>
        /// <param name="itemDto">Данные для обновления позиции.</param>
        /// <returns>Обновлённая корзина пользователя.</returns>

        // PUT api/<CartItemsController>/5
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpPut("{id}")]
        public async Task<ActionResult<CartDto>> Put(int id, [FromBody]NewCartItemDto itemDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var cart = await _cartService.UpdateItemFromcartAsync(itemDto);
            return Ok(cart);
        }


        /// <summary>
        /// Удаляет позицию из корзины.
        /// </summary>
        /// <param name="id">Идентификатор позиции корзины для удаления.</param>
        /// <returns>Обновлённая корзина пользователя.</returns>

        // DELETE api/<CartItemsController>/5
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpDelete("{id}")]
        
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
