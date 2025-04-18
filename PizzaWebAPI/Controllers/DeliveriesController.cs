using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
    {
        private readonly DeliveryService _deliveryService;
        private readonly UserManager<User> _userManager;

        public DeliveriesController(
            DeliveryService deliveryService,
            UserManager<User> userManager)
        {
            _deliveryService = deliveryService;
            _userManager = userManager;
        }

        // Для менеджеров: просмотр всех доставок
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllDeliveries()
        {
            var deliveries = await _deliveryService.GetDeliveriesAsync();
            return Ok(deliveries);
        }

        // Для клиентов: получение доставки своего заказа
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var deliveries = await _deliveryService.GetDeliveriesByOrderIdAsync(orderId);
            return Ok(deliveries.FirstOrDefault()); // Предполагаем 1 доставку на заказ
        }

        // Для курьеров: текущие активные доставки
        [HttpGet("courier")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> GetCourierDeliveries()
        {
            var courier = await _userManager.GetUserAsync(User);
            var deliveries = await _deliveryService.GetDeliveriesAsync();
            return Ok(deliveries.Where(d => d.CourierId == courier.Id));
        }

        //// Для менеджеров: создание новой доставки
        //[HttpPost]
        //[Authorize(Roles = "Manager")]
        //public async Task<IActionResult> CreateDelivery(
        //    [FromBody] CreateDeliveryDto createDto)
        //{
        //    await _deliveryService.AddDeliveryAsync(createDto);
        //    return CreatedAtAction(nameof(GetDelivery), new { id = createDto.Id }, createDto);
        //}

        // Для курьеров: обновление статуса доставки
        [HttpPut("{id}")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> UpdateDelivery(
            int id,
            [FromBody] UpdateDeliveryDto updateDto)
        {
            var courier = await _userManager.GetUserAsync(User);
            var delivery = await _deliveryService.GetDeliveryByIdAsync(id);

            if (delivery.CourierId != courier.Id)
                return Forbid();

            await _deliveryService.UpdateDeliveryAsync(updateDto);
            return NoContent();
        }

        // Для всех авторизованных: просмотр конкретной доставки
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDelivery(int id)
        {
            var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);

            // Проверка прав доступа
            if (User.IsInRole("Customer") && delivery.Order.ClientId != user.Id)
                return Forbid();

            if (User.IsInRole("Courier") && delivery.CourierId != user.Id)
                return Forbid();

            return Ok(delivery);
        }
    }
}
