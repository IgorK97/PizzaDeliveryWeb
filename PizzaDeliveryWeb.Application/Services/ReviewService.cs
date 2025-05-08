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
    /// <summary>
    /// Сервис для управления отзывами клиентов.
    /// Предоставляет методы для создания, получения, обновления и удаления отзывов.
    /// </summary>
    public class ReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        /// <summary>
        /// Конструктор сервиса отзывов.
        /// </summary>
        /// <param name="reviewRepository">Интерфейс репозитория отзывов</param>

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// Получает отзыв по его идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор отзыва</param>
        /// <returns>Объект ReviewDto</returns>
        /// <exception cref="Exception">Выбрасывается, если отзыв не найден</exception>
        public async Task<ReviewDto> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
                throw new Exception("Такого отзыва нет.");

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

        /// <summary>
        /// Получает все отзывы, связанные с определённым заказом.
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <returns>Список объектов ReviewDto</returns>
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

        /// <summary>
        /// Добавляет новый отзыв от клиента.
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <param name="customerId">Идентификатор клиента</param>
        /// <param name="content">Содержимое отзыва</param>
        /// <param name="rating">Оценка отзыва (например, от 1 до 5)</param>

        public async Task AddReviewAsync(int orderId, string customerId, string content, int rating)
        {
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


        /// <summary>
        /// Обновляет существующий отзыв.
        /// </summary>
        /// <param name="reviewId">Идентификатор отзыва</param>
        /// <param name="content">Новое содержимое отзыва</param>
        /// <param name="rating">Новая оценка</param>
        /// <exception cref="Exception">Выбрасывается, если отзыв не найден</exception>

        public async Task UpdateReviewAsync(int reviewId, string content, int rating)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null)
                throw new Exception("Отзыв не найден");

            review.Content = content;
            review.Rating = rating;
            review.CreatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateReviewAsync(review);
        }


        /// <summary>
        /// Удаляет отзыв по его идентификатору.
        /// </summary>
        /// <param name="reviewId">Идентификатор отзыва</param>
        public async Task DeleteReviewAsync(int reviewId)
        {
            await _reviewRepository.DeleteReviewAsync(reviewId);
        }
    }
}
