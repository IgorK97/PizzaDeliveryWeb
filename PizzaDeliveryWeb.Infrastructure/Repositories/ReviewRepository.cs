using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaDeliveryWeb.Infrastructure.Data;

namespace PizzaDeliveryWeb.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly PizzaDeliveringContext _context;

        public ReviewRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }

        public async Task<Review> GetReviewByIdAsync(int id)
        {
            return await _context.Reviews.FindAsync(id);
        }

        public async Task<IEnumerable<Review>> GetReviewsByOrderIdAsync(int orderId)
        {
            return await _context.Reviews
                .Where(r => r.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByCustomerIdAsync(string customerId)
        {
            return await _context.Reviews
                .Where(r => r.ClientId == customerId)
                .ToListAsync();
        }

        public async Task AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReviewAsync(Review review)
        {
            Order order = await _context.Orders.Where(o => o.Id == review.OrderId).FirstOrDefaultAsync();
            //User usr = await _userManager.GetUserAsync(HttpContext.User);
            User user = await _context.Users.Where(u => u.Id == review.ClientId).FirstOrDefaultAsync();

            if (order != null && user != null)
            {
                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await GetReviewByIdAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
}
