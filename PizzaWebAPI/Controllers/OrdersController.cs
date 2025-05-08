using System.Collections;
using System.Security.Claims;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpGet]
        //[Authorize(Roles = "client")]
        
        public async Task<ActionResult<IEnumerable<ShortOrderDto>>> GetMyOrders(
            //[FromQuery] int? lastId = 0,
            //[FromQuery] int pageSize = 10
            )
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();
                //if (lastId == -1)
                //    lastId = null;
                var orders = await _orderService.GetClientOrderHistoryAsync(user.Id, null, null);
                //var hasMore = orders.Any() && orders.Last().Id < lastId;
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
            //[FromQuery] int? lastId = 0,
            //[FromQuery] int pageSize = 10
            )
        {
            //var orders = status.HasValue
            //? await _orderService.GetOrdersByStatusAsync(status.Value, lastId, pageSize)
            //: await _orderService.GetAllActiveOrdersAsync();
            try
            {
                //int? myLastId = lastId;
                //if (lastId == -1)
                //    myLastId = null;
                var orders = status != 0 ? await _orderService.GetOrdersByStatusAsync((OrderStatusEnum)status, null, null)
                : await _orderService.GetAllActiveOrdersAsync(null, null);
                //var hasMore = orders.Any() && orders.Last().Id < lastId;
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "courier")]
        [HttpGet("courier")]
       
        public async Task<IActionResult> GetCourierOrders([FromQuery] int status = 0
            //[FromQuery] int? lastId = 0,
            //[FromQuery] int pageSize = 10
            )
        {
            try
            {
                //var user = await _userManager.GetUserAsync(User);
                //if (user == null) return Unauthorized();

                var userId = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (userId == null) return Unauthorized();

                //var userId = Guid.Parse(userIdClaim.Value);
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return Unauthorized();


                //int? myLastId = lastId;
                //if (lastId == -1)
                //    myLastId = null;
                OrderStatusEnum? state = status != 0 ? (OrderStatusEnum)status : null;
                var orders = await _orderService.GetCourierActiveOrdersAsync(user.Id, state,
                    null, null);
                    
                //var hasMore = orders.Any() && orders.Last().Id < lastId;
                return Ok(new
                {
                    Items = orders,
                    HasMore =false,
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "client")]
        [HttpPatch("{id}/cancel")]
        //[Authorize(Roles ="client")]
        public async Task<ActionResult<ShortOrderDto>> CancelOrder(int id)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            try
            {
                var order = await _orderService.CancelOrderAsync(id);
                order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }

            catch (NotFoundException ex)
            {
                _logger.LogWarning("Попытка отменить несуществующий заказ. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
    id, user.Id, ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Попытка отмены заказа, который нельзя отменить. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
                    id, user.Id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка при отмене заказа в БД. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
                    id, user.Id, ex.Message);
                return StatusCode(500, "Ошибка сервера при отмене заказа. Попробуйте позже.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непредвиденная ошибка при отмене заказа. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
                    id, user.Id, ex.Message);
                return StatusCode(500, "Произошла непредвиденная ошибка.");
            }


        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]
        [HttpPatch("{id}/accept")]
        //[Authorize(Roles ="manager")]
        public async Task<ActionResult<ShortOrderDto>> AcceptOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _orderService.AcceptOrderAsync(id, user.Id);
                var order = await _orderService.GetOrderByIdAsync(id);
                _logger.LogInformation("Менеджер {managerId} принял заказ {orderId}", user.Id, id);
                return Ok(order);
            }
            catch(NotFoundException ex)
            {
                _logger.LogWarning("Заказ не найден: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Невозможно принять заказ {orderId}: {ErrorMessage}", id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка БД при принятии заказа {orderId} менеджером {managerId}: {ErrorMessage}",
                    id, user.Id, ex.Message);
                return StatusCode(500, "Ошибка сервера при принятии заказа.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Неизвестная ошибка при принятии заказа {orderId} менеджером {managerId}: {ErrorMessage}",
                    id, user.Id, ex.Message);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "manager")]
        [HttpPatch("{id}/to-delivery")]
        //[Authorize]
        public async Task<ActionResult<ShortOrderDto>> StartDelivery(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _orderService.TransferToDelivery(id);
                var order = await _orderService.GetOrderByIdAsync(id);
                _logger.LogInformation("Заказ {OrderId} был передан в доставку менеджером {ManagerId}", id, user.Id);
                return Ok(order);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Не удалось передать заказ {OrderId} в доставку: не найден. Менеджер: {ManagerId}", id, user.Id);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Невозможно передать заказ {OrderId} в доставку: {ErrorMessage}. Менеджер: {ManagerId}", id, ex.Message, user.Id);
                return Conflict(ex.Message);
            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка базы данных при передаче заказа {OrderId} в доставку менеджером {ManagerId}: {ErrorMessage}", id, user.Id, ex.Message);
                return StatusCode(500, "Не удалось сохранить изменения. Попробуйте позже.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при передаче заказа {OrderId} в доставку менеджером {ManagerId}", id, user.Id);
                return StatusCode(500, "Произошла внутренняя ошибка сервера.");
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "courier")]
        [HttpPatch("{id}/choose")]
        //[Authorize]
        public async Task<ActionResult<ShortOrderDto>> TakeOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _orderService.TakeOrder(id, user.Id);
                var order = await _orderService.GetOrderByIdAsync(id);
                _logger.LogInformation("Курьер {CourierId} принял заказ {OrderId} на доставку.", user.Id, id);
                return Ok(order);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Не удалось найти заказ {OrderId} для курьера {CourierId}. Ошибка: {Error}", id, user.Id, ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Невозможно взять заказ {OrderId} курьером {CourierId}. Ошибка: {Error}", id, user.Id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка базы данных при принятии заказа {OrderId} курьером {CourierId}. Ошибка: {Error}", id, user.Id, ex.Message);
                return StatusCode(500, "Ошибка при сохранении данных. Попробуйте позже.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при принятии заказа {OrderId} курьером {CourierId}.", id, user.Id);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "courier")]
        [HttpPatch("{id}/complete")]
        //[Authorize]
        public async Task<ActionResult<ShortOrderDto>> CompleteDelivery(SetDeliveryModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _orderService.CompleteDeliveryAsync(model.OrderId, model.Status, model.Comment);
                var order = await _orderService.GetOrderByIdAsync(model.OrderId);
                _logger.LogInformation("Курьер {CourierId} доставил заказ {OrderId}, статус успеха: {status}, сообщение: {message}.", user.Id, model.OrderId,
                    model.Status, model.Comment);
                return Ok(order);
            }
            catch(NotFoundException ex)
            {
                _logger.LogWarning("Не удалось найти ресурс: {Resource} с ID {Id}. Ошибка: {ErrorMessage}",
            ex.EntityType, ex.Key, ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Невозможность завершения доставки для заказа с ID {OrderId}. Ошибка: {ErrorMessage}",
                    model.OrderId, ex.Message);
                return Conflict(new { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Ошибка при указании статуса доставки для заказа с ID {OrderId}. Ошибка: {ErrorMessage}",
                    model.OrderId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (MyDbException ex)
            {
                _logger.LogError("Ошибка при сохранении данных о доставке для заказа с ID {OrderId}. Ошибка: {ErrorMessage}",
                    model.OrderId, ex.Message);
                return StatusCode(500, "Ошибка сервера при обработке данных о доставке. Попробуйте позже.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Неизвестная ошибка при завершении доставки для заказа с ID {OrderId}. Ошибка: {ErrorMessage}",
                    model.OrderId, ex.Message);
                return StatusCode(500, "Ошибка сервера. Попробуйте позже.");
            }
        }


        // DELETE api/<OrdersController>/5
    //    [HttpDelete("{id}")]
    //    public async Task<ActionResult<OrderDto>> Delete(int id)
    //    {
    //        var user = await _userManager.GetUserAsync(User);
    //        if (user == null) return Unauthorized();
    //        try
    //        {
    //            var order = await _orderService.CancelOrderAsync(id);
    //            return Ok(order);
    //        }
            
    //        catch(NotFoundException ex)
    //        {
    //            _logger.LogWarning("Попытка отменить несуществующий заказ. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
    //id, user.Id, ex.Message);
    //            return NotFound(ex.Message);
    //        }
    //        catch (InvalidOperationException ex)
    //        {
    //            _logger.LogWarning("Попытка отмены заказа, который нельзя отменить. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
    //                id, user.Id, ex.Message);
    //            return BadRequest(ex.Message);
    //        }
    //        catch (MyDbException ex)
    //        {
    //            _logger.LogError("Ошибка при отмене заказа в БД. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
    //                id, user.Id, ex.Message);
    //            return StatusCode(500, "Ошибка сервера при отмене заказа. Попробуйте позже.");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError("Непредвиденная ошибка при отмене заказа. ЗаказId: {OrderId}, ПользовательId: {UserId}. Ошибка: {Error}",
    //                id, user.Id, ex.Message);
    //            return StatusCode(500, "Произошла непредвиденная ошибка.");
    //        }

    //    }
    }
}
