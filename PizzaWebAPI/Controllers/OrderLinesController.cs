using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderLinesController : ControllerBase
    {
        private readonly OrderLineService _orderLineService;
        public OrderLinesController(OrderLineService orderLineService)
        {
            _orderLineService = orderLineService;
        }

        // GET: api/<OrderLinesController>
        // Получить все строки заказа по ID заказа
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderLineDto>>> GetOrderLinesByOrderIdAsync(int orderId)
        {
            try
            {
                var result = await _orderLineService.GetOrderLinesByOrderIdAsync(orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при получении строк заказов: {ex.Message}");
            }
        }

        // GET api/<OrderLinesController>/5
        // Получить строку заказа по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderLineDto>> GetOrderLineByIdAsync(int id)
        {
            try
            {
                var result = await _orderLineService.GetOrderLineByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound($"Ошибка при получении строки заказа с ID {id}: {ex.Message}");
            }
        }

        // POST api/<OrderLinesController>
        // Создать новую строку заказа
        [HttpPost]
        public async Task<ActionResult<OrderLineDto>> CreateOrderLineAsync([FromBody] CreateOrderLineDto orderLineDto)
        {
            await _orderLineService.AddOrderLineAsync(orderLineDto);
            return Ok();
            //return CreatedAtAction(nameof(GetOrderLineByIdAsync), new { id = createdOrderLine.Id }, createdOrderLine);
          
        }

        // PUT api/<OrderLinesController>/5
        // Обновить строку заказа
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderLineAsync(int id, [FromBody] CreateOrderLineDto orderLineDto)
        {
            try
            {
                if (id != orderLineDto.Id)
                {
                    return BadRequest("ID в URL и в теле запроса не совпадают");
                }

                await _orderLineService.UpdateOrderLineAsync(orderLineDto);
                return NoContent(); 
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обновлении строки заказа: {ex.Message}");
            }
        }

        // DELETE api/<OrderLinesController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderLineAsync(int id)
        {
            await _orderLineService.DeleteOrderLineAsync(id);
            return NoContent();

        }
    }
}
