using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.API.Models;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    /// <summary>
    /// Контроллер для работы с отзывами.
    /// Позволяет получать, добавлять, обновлять и удалять отзывы.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        public readonly ReviewService _reviewService;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Конструктор контроллера.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей для работы с идентификацией пользователей.</param>
        /// <param name="reviewService">Сервис для работы с отзывами.</param>
        /// <param name="logger">Логгер для записи действий в контроллере.</param>
        public ReviewsController(UserManager<User> userManager, ReviewService reviewService)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        /// <summary>
        /// Получить список отзывов по ID заказа.
        /// </summary>
        /// <param name="orderId">ID заказа для получения отзывов.</param>
        /// <returns>Список отзывов, относящихся к указанному заказу.</returns>
        // GET: api/<ReviewsController>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByOrderId(int orderId)
        {
            var reviews = await _reviewService.GetReviewsByOrderIdAsync(orderId);
            if (reviews == null)
                NotFound();
            return Ok(reviews);
        }


        /// <summary>
        /// Получить отзыв по ID.
        /// </summary>
        /// <param name="id">ID отзыва для получения.</param>
        /// <returns>Отзыв с указанным ID.</returns>
        // GET api/<ReviewsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();
            return review;
        }


        /// <summary>
        /// Добавить новый отзыв.
        /// </summary>
        /// <param name="model">Модель отзыва с информацией о контенте и рейтинге.</param>
        /// <returns>Ответ о результате добавления отзыва.</returns>
        // POST api/<ReviewsController>
        [HttpPost]
        public async Task<ActionResult> AddReview([FromBody]AddReviewModel model)
        {
            User usr = await _userManager.GetUserAsync(HttpContext.User);

            await _reviewService.AddReviewAsync(model.OrderId, usr.Id, model.Content, model.Rating);
            return Ok();
        }


        /// <summary>
        /// Обновить отзыв.
        /// </summary>
        /// <param name="model">Модель для обновления отзыва с новым контентом и рейтингом.</param>
        /// <returns>Ответ о результате обновления отзыва.</returns>
        // PUT api/<ReviewsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Updatereview([FromBody]UpdateReviewModel model)
        {
            await _reviewService.UpdateReviewAsync(model.Id, model.Content, model.Rating);
            return Ok();
        }


        /// <summary>
        /// Удалить отзыв по ID.
        /// </summary>
        /// <param name="id">ID отзыва для удаления.</param>
        /// <returns>Ответ о результате удаления отзыва.</returns>
        // DELETE api/<ReviewsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return Ok();
        }
    }
}
