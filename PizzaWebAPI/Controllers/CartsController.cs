﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.API.Models;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {


        private readonly UserManager<User> _userManager;
        private readonly CartService _cartService;
        private readonly ILogger<CartsController> _logger;


        public CartsController(UserManager<User> userManager, CartService cartService, ILogger<CartsController> logger)
        {
            _userManager = userManager;
            _cartService = cartService;
            _logger = logger;
        }



        // GET: api/<CartsController>
        [HttpGet]
        [Authorize(Roles ="client")]
        
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var cart = await _cartService.GetOrCreateCartAsync(user.Id);
            _logger.LogInformation("Возвращение корзины");
            return Ok(cart);
            //return new string[] { "value1", "value2" };
        }
        [HttpPut]
        [Authorize(Roles ="client")]
        public async Task<ActionResult<CartDto>> UpdateCart([FromBody] CartDto cartDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            try
            {
                var updatedCart = await _cartService.UpdateCartAsync(cartDto);
                return Ok(updatedCart);
            }
            catch(DirectoryNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("items")]
        [Authorize]
        public async Task<ActionResult<CartDto>> AddItemToCart([FromBody] NewCartItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) 
                    return Unauthorized();
                var cart = await _cartService.AddNewItemToCartAsync(itemDto);
                return Ok(cart);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpPost("submit")]
        [Authorize]
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
            catch (Exception ex)
            {

                var currentCart = await _cartService.GetOrCreateCartAsync(user.Id);
                return Conflict(new CartSubmitResult
                {
                    Success=false,
                    Message = "Оформить заказ не удалось. Корзина обновлена. Проверьте ее еще раз",
                    UpdatedCart = currentCart
                });
            }
            
        }


    }
}
