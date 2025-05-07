using System.Collections;
using System.Security.Claims;
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
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly DeliveryService _deliveryService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(UserManager<User> userManager, IWebHostEnvironment env, OrderService orderService, 
            DeliveryService deliveryService,
            ILogger<OrdersController> logger)
        {
            _userManager = userManager;

            _orderService = orderService;
            _env = env;
            _deliveryService = deliveryService;
            _logger = logger;
        }


        //[HttpGet("my")]
        [HttpGet]
        [Authorize(Roles = "client")]
        
        public async Task<ActionResult<IEnumerable<ShortOrderDto>>> GetMyOrders()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var orders = await _orderService.GetClientOrderHistoryAsync(user.Id, null, 50);
                //return Ok(orders);
                return Ok(new
                {
                    Items = orders,
                    HasMore = false,
                    LastId = 0
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запросе заказов");
                return StatusCode(500, "Ошибка сервера");
            }
            //var hasMore = orders.Any() && orders.Last().Id > lastId;
            //var hasMore = true;

            //return Ok(new
            //{
            //    Items = orders,
            //    LastId = orders.LastOrDefault()?.Id ?? lastId,
            //    HasMore = hasMore
            //});
        }

        [HttpGet("manager")]
        //[Authorize(Roles = "manager")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int status=0
            //int lastId,
            //int pageSize = 10
            )
        {
            //var orders = status.HasValue
            //? await _orderService.GetOrdersByStatusAsync(status.Value, lastId, pageSize)
            //: await _orderService.GetAllActiveOrdersAsync();
            try
            {
                var orders = status != 0 ? await _orderService.GetOrdersByStatusAsync((OrderStatusEnum)status, null, 10)
                : await _orderService.GetAllActiveOrdersAsync();
                return Ok(new
                {
                    Items = orders,
                    HasMore = false,
                    LastId = 0
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запросе заказов");
                return StatusCode(500, "Ошибка сервера");
            }
        }


        [HttpGet("courier")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> GetCourierOrders([FromQuery] int status = 0)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var orders = await _orderService.GetCourierActiveOrdersAsync(user.Id, (OrderStatusEnum)status);
                return Ok(new
                {
                    Items = orders,
                    HasMore = false,
                    LastId = 0
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запросе заказов");
                return StatusCode(500, "Ошибка сервера");
            }
            //return Ok();
        }


        [HttpPatch("{id}/cancel")]
        [Authorize]
        public async Task<ActionResult<ShortOrderDto>> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            //// Дополнительная проверка принадлежности заказа
            //if (order.ClientId != user.Id) return Forbid();

            await _orderService.CancelOrderAsync(id);
            var order = await _orderService.GetOrderByIdAsync(id);

            return Ok(order);
        }


        [HttpPatch("{id}/accept")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult<ShortOrderDto>> AcceptOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _orderService.AcceptOrderAsync(id, user.Id);
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }


        [HttpPatch("{id}/to-delivery")]
        [Authorize]
        public async Task<ActionResult<ShortOrderDto>> StartDelivery(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _orderService.TransferToDelivery(id);
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }

        [HttpPatch("{id}/choose")]
        [Authorize]
        public async Task<ActionResult<ShortOrderDto>> TakeOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _orderService.TakeOrder(id, user.Id);
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }


        [HttpPatch("{id}/complete")]
        [Authorize]
        public async Task<ActionResult<ShortOrderDto>> CompleteDelivery(SetDeliveryModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _orderService.CompleteDeliveryAsync(model.OrderId, model.Status, model.Comment);
            var order = await _orderService.GetOrderByIdAsync(model.OrderId);
            return Ok(order);
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
        public async Task<ActionResult<OrderDto>> Post([FromBody]ConfirmOrderDto order)
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
