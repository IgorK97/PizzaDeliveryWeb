using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryWeb.Application.Services
{
    public class ReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
                throw new Exception("Review not found.");

            return new ReviewDto
            {
                Id = review.Id,
                OrderId = review.OrderId,
                ClientId = review.ClientId,
                Content = review.Content,
                Rating = review.Rating,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByOrderIdAsync(int orderId)
        {
            var reviews = await _reviewRepository.GetReviewsByOrderIdAsync(orderId);
            return reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                OrderId = r.OrderId,
                ClientId = r.ClientId,
                Content = r.Content,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task AddReviewAsync(int orderId, string customerId, string content, int rating)
        {
            // Здесь могут быть дополнительные проверки (например, что заказ завершен и пользователь может оставить отзыв)
            var review = new Review
            {
                Id = 0,
                OrderId = orderId,
                ClientId = customerId,
                Content = content,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddReviewAsync(review);
        }

        public async Task UpdateReviewAsync(int reviewId, string content, int rating)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null)
                throw new Exception("Review not found.");

            review.Content = content;
            review.Rating = rating;
            review.CreatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateReviewAsync(review);
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            await _reviewRepository.DeleteReviewAsync(reviewId);
        }
    }
}
