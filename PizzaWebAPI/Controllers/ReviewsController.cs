using Microsoft.AspNetCore.Mvc;
using PizzaDeliveryWeb.API.Models;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaDeliveryWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        public readonly ReviewService _reviewService;
        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/<ReviewsController>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByOrderId(int orderId)
        {
            var reviews = await _reviewService.GetReviewsByOrderIdAsync(orderId);
            if (reviews == null)
                NotFound();
            return Ok(reviews);
        }

        // GET api/<ReviewsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();
            return review;
        }

        // POST api/<ReviewsController>
        [HttpPost]
        public async Task<ActionResult> AddReview([FromBody]AddReviewModel model)
        {
            await _reviewService.AddReviewAsync(model.OrderId, model.ClientId, model.Content, model.Rating);
            return Ok();
        }

        // PUT api/<ReviewsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Updatereview([FromBody]UpdateReviewModel model)
        {
            await _reviewService.UpdateReviewAsync(model.Id, model.Content, model.Rating);
            return Ok();
        }

        // DELETE api/<ReviewsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return Ok();
        }
    }
}
