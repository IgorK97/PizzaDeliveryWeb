using System.Security.Claims;
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
        /// для клиента только ИСТОРИЯ ЗАКАЗОВ (пока что)
        /// </summary>
        /// <returns></returns>
        // GET: api/<OrdersController>
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
        //    [FromQuery] int? lastId = null,
        //    [FromQuery] int pageSize=10)
        //{
        //    User usr = await _userManager.GetUserAsync(HttpContext.User);
        //    if (usr == null)
        //    {
        //        return Unauthorized(new { message = "Вы Гость. Пожалуйста, выполните вход" });
        //    }
        //    var roles = User.Claims
        //        .Where(c => c.Type == ClaimTypes.Role)
        //        .Select(c => c.Value)
        //        .ToList();
        ////    var myr = User.Claims
        ////.FirstOrDefault(c => c.Type == "SelectedRole")?.Value;
        //    IEnumerable<OrderDto> orders;
        //    if (roles.Contains("client"))
        //    {
        //        orders = await _orderService.GetClientOrdersAsync(usr.Id);
        //    }
        //    else if (roles.Contains("courier"))
        //    {
        //        //IEnumerable<OrderDto> courierOrders;
        //        orders = await _orderService.GetCourierOrdersAsync(usr.Id);
        //    }
        //    else if (roles.Contains("admin"))
        //    {
        //        orders = await _orderService.GetAllActiveOrdersAsync();
        //    }
        //    else
        //    {
        //        return Forbid();
        //    }
        //        //var orders = await _orderService.GetOrdersAsync();
        //        return Ok(orders);
        //}

        [HttpGet("my")]
        [Authorize(Roles ="client")]
        public async Task<IActionResult> GetMyOrders(int? lastId, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            return Ok(await _orderService.GetClientOrderHistoryAsync(user.Id));
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] OrderStatusEnum? status,
            int? lastId,
            int pageSize = 10)
        {
            var orders = status.HasValue
            ? await _orderService.GetOrdersByStatusAsync(status.Value, lastId, pageSize)
            : await _orderService.GetAllActiveOrdersAsync();

            return Ok(orders);
        }


        [HttpGet("courier")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> GetCourierOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            return Ok(await _orderService.GetCourierActiveOrdersAsync(user.Id));
            //return Ok();
        }


        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Дополнительная проверка принадлежности заказа
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order.ClientId != user.Id) return Forbid();

            await _orderService.CancelOrderAsync(id);
            return NoContent();
        }


        [HttpPost("{id}/accept")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _orderService.AcceptOrderAsync(id, user.Id);
            return NoContent();
        }


        [HttpPost("{id}/start-delivery")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> StartDelivery(int id)
        {
            await _orderService.StartDeliveryAsync(id);
            return NoContent();
        }


        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> CompleteDelivery(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _orderService.CompleteDeliveryAsync(id, user.Id);
            return NoContent();
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

        //[HttpGet("cart")]
        //public async Task<ActionResult<OrderDto>> Getcart()
        //{
        //    User usr = await _userManager.GetUserAsync(HttpContext.User);
        //    if (usr == null)
        //    {
        //        return Unauthorized(new { message = "Вы Гость. Пожалуйста, выполните вход" });
        //    }
        //    var cart = await _orderService.GetOrCreateCartAsync(usr.Id);
        //    return Ok(cart);
        //}

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
            return await _orderService.SubmitOrderAsync(order.Id);
        }

        // PUT api/<OrdersController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{

        //}

        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<OrderDto>> Delete(int id)
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
