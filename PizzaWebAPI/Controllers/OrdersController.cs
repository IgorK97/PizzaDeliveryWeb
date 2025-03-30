using System.Security.Claims;
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
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly DeliveryService _deliveryService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;
        public OrdersController(UserManager<User> userManager, IWebHostEnvironment env, OrderService orderService, 
            DeliveryService deliveryService)
        {
            _userManager = userManager;

            _orderService = orderService;
            _env = env;
            _deliveryService = deliveryService;
        }
        /// <summary>
        /// Получить список заказов (по роли пользователя)
        /// </summary>
        /// <returns></returns>
        // GET: api/<OrdersController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            User usr = await _userManager.GetUserAsync(HttpContext.User);
            if (usr == null)
            {
                return Unauthorized(new { message = "Вы Гость. Пожалуйста, выполните вход" });
            }
            var roles = User.Claims
                .Where(c => c.Type == "SelectedRole")
                .Select(c => c.Value)
                .ToList();
            IEnumerable<OrderDto> orders;
            if (roles.Contains("client"))
            {
                orders = await _orderService.GetClientOrdersAsync(usr.Id);
            }
            else if (roles.Contains("courier"))
            {
                //IEnumerable<OrderDto> courierOrders;
                orders = await _orderService.GetCourierOrdersAsync(usr.Id);
            }
            else if (roles.Contains("admin"))
            {
                orders = await _orderService.GetAllActiveOrdersAsync();
            }
            else
            {
                return Forbid();
            }
                //var orders = await _orderService.GetOrdersAsync();
                return Ok(orders);
        }

        // GET api/<OrdersController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpGet("cart")]
        public async Task<ActionResult<OrderDto>> Getcart()
        {
            User usr = await _userManager.GetUserAsync(HttpContext.User);
            if (usr == null)
            {
                return Unauthorized(new { message = "Вы Гость. Пожалуйста, выполните вход" });
            }
            var cart = await _orderService.GetOrCreateCartAsync(usr.Id);
            return Ok(cart);
        }

        // POST api/<OrdersController>
        [HttpPost("confirm/{id}")]
        public async Task<ActionResult<OrderDto>> Post([FromBody]ShortOrderDto order)
        {
            User usr = await _userManager.GetUserAsync(HttpContext.User);
            if (usr == null)
            {

                return Unauthorized(new { message = "Вы Гость. Пожалуйста, выполните вход" });

            }
            order.ClientId = usr.Id;
            return await _orderService.SubmitOrderAsync(order);
        }

        // PUT api/<OrdersController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{

        //}

        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<PizzaDto>> Delete(int id)
        {
            var order = await _orderService.CancelOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
    }
}
