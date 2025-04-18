using System.ComponentModel.DataAnnotations;
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
    public class CartsController : ControllerBase
    {


        private readonly UserManager<User> _userManager;
        private readonly CartService _cartService;


        public CartsController(UserManager<User> userManager, CartService cartService)
        {
            _userManager = userManager;
            _cartService = cartService;
        }



        // GET: api/<CartsController>
        [HttpGet]
        [Authorize(Roles ="client")]
        
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var cart = await _cartService.GetOrCreateCartAsync(user.Id);
           
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

        [HttpPost("submit")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> SubmitCart()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                await _cartService.SubmitCartAsync(user.Id);

                var cart = await _cartService.GetOrCreateCartAsync(user.Id);

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }


    }
}
