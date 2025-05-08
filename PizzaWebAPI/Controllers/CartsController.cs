using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.API.Models;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.MyExceptions;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    /// <summary>
    /// Контроллер для управления корзиной пользователя: получение содержимого, добавление товаров и оформление заказа.
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {


        private readonly UserManager<User> _userManager;
        private readonly CartService _cartService;
        private readonly ILogger<CartsController> _logger;

        /// <summary>
        /// Конструктор контроллера корзины.
        /// </summary>
        /// <param name="userManager">Сервис управления пользователями.</param>
        /// <param name="cartService">Сервис корзины.</param>
        /// <param name="logger">Сервис логирования.</param>

        public CartsController(UserManager<User> userManager, CartService cartService, ILogger<CartsController> logger)
        {
            _userManager = userManager;
            _cartService = cartService;
            _logger = logger;
        }


        /// <summary>
        /// Получает текущую корзину пользователя. Если корзина отсутствует — создаёт новую.
        /// </summary>
        /// <returns>Текущая корзина пользователя.</returns>

        // GET: api/<CartsController>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpGet]
        
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            try
            {
                var cart = await _cartService.GetOrCreateCartAsync(user.Id);
                _logger.LogInformation("Возвращение корзины");
                return Ok(cart);
            }
            catch (CartNotFoundException ex)
            {
                _logger.LogWarning("Корзина не найдена для пользователя {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);
                return NotFound("Корзина не найдена.");
            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка при работе с базой данных у пользователя {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);
                return StatusCode(500, "Произошла ошибка при получении корзины. Попробуйте позже.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Неизвестная ошибка при получении корзины для пользователя {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);
                return StatusCode(500, "Произошла непредвиденная ошибка. Попробуйте позже.");
            }

        }


        /// <summary>
        /// Добавляет новый товар в корзину пользователя.
        /// </summary>
        /// <param name="itemDto">Данные о добавляемом товаре.</param>
        /// <returns>Обновлённая корзина пользователя.</returns>

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpPost("items")]
        
        public async Task<ActionResult<CartDto>> AddItemToCart([FromBody] NewCartItemDto itemDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            try
            {
                
                var cart = await _cartService.AddNewItemToCartAsync(itemDto);
                _logger.LogInformation("Пользователем {userId} был добавлен товар в корзину: Cart = {CartId}", 
                    user.Id, itemDto.CartId);
                return Ok(cart);
            }
            catch(ArgumentException ex)
            {
                _logger.LogWarning("Некорреткный запрос при добавлении позиции в корзину пользвоателем {userId}. Ошибка: {ErrorMessage}",
                    user.Id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch(NotFoundException ex)
            {
                _logger.LogWarning("Ресурс не найден: {ResourseName}, ID: {Id}", ex.EntityType, ex.Key);
                return NotFound(ex.Message);


            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка на стороне БД: {ErrorMessage}", ex.Message);
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Внутренняя ошибка сервера: {ErrorMessage}", ex.Message);

                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }


        /// <summary>
        /// Оформляет заказ на основе текущей корзины пользователя.
        /// </summary>
        /// <param name="newOrder">Модель данных для оформления заказа.</param>
        /// <returns>Результат оформления заказа и обновлённая корзина.</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpPost("submit")]
        
        public async Task<ActionResult<OrderDto>> SubmitCart([FromBody] SubmitOrderModel newOrder)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            try
            {
                

                await _cartService.SubmitCartAsync(user.Id, newOrder.Price, newOrder.Address);

                var cart = await _cartService.GetOrCreateCartAsync(user.Id);

                return Ok(new CartSubmitResult
                {
                    Success=true,
                    Message="Заказ успешно оформлен!",
                    UpdatedCart=cart
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Некорреткный запрос при оформлении заказа пользвоателем {userId}. Ошибка: {ErrorMessage}",
                    user.Id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch(CartNotFoundException ex)
            {
                _logger.LogWarning("Корзина для пользователя с ID не найдена: {Id}", user.Id);
                return NotFound(ex.Message);
            }
            catch(OutdatedCartException ex)
            {
                _logger.LogWarning("Корзина устарела для пользователя с ID {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);

                CartDto updatedCart;
                try
                {
                    updatedCart = await _cartService.GetOrCreateCartAsync(user.Id);
                }
                catch (Exception fetchEx)
                {
                    _logger.LogError("Не удалось получить или создать корзину после ошибки устаревания. Пользователь: {userId}, Ошибка: {ErrorMessage}", user.Id, fetchEx.Message);
                    return StatusCode(500, "Ошибка при повторном получении корзины. Попробуйте позже.");
                }

                return Conflict(new CartSubmitResult
                {
                    Success = false,
                    Message = "Корзина устарела, пожалуйста, обновите корзину и попробуйте снова.",
                    UpdatedCart = updatedCart
                });
            }
            catch(EmptyCartException ex)
            {
                _logger.LogWarning("Корзина пуста для пользователя с ID {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);
                return BadRequest("Корзина пуста, добавьте товары перед оформлением заказа.");
            }
            catch(MyDbException ex)
            {
                _logger.LogError("Ошибка базы данных при оформлении заказа пользователем с ID {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);
                return StatusCode(500, "Ошибка сервера при сохранении данных. Пожалуйста, попробуйте позже.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка сервера при оформлении заказа пользователем с ID {userId}. Ошибка: {ErrorMessage}", user.Id, ex.Message);
                return StatusCode(500, "Ошибка сервера: " + ex.Message);
                
            }
            
        }


    }
}
