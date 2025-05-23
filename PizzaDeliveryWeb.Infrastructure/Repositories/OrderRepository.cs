﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;
using PizzaDeliveryWeb.Infrastructure.Data;

namespace PizzaDeliveryWeb.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PizzaDeliveringContext _context;

        public OrderRepository(PizzaDeliveringContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(p => p.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o=>o.OrderLines)
                    .ThenInclude(ol=>ol.Ingredients)
                .Include(o=>o.OrderLines)
                    .ThenInclude(ol=>ol.PizzaSize)
                .Include(o => o.Delivery)
                .Include(o => o.DelStatus)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<IEnumerable<OrderLine>> GetNotPlacedOrderLinesWithIngredientAsync(int ingredientId)
        {
    
            return await _context.OrderLines
                .AsNoTracking()
                .Include(ol => ol.Order)
                .Include(ol=>ol.Pizza)
                    .ThenInclude(p=>p.Ingredients)
                    .Where(ol=>ol.Order.DelStatusId==(int)OrderStatusEnum.NotPlaced &&
                    ol.Pizza.Ingredients.Any(i=>i.Id==ingredientId))
                .ToListAsync();

  
        }


        //public async Task<List<Delivery>> GetDeliveriesByOrderIdAsync(int orderId)
        //{
        //    return await _context.Deliveries.Where(t => t.OrderId == orderId).ToListAsync();
        //}

        public async Task<IEnumerable<Order>> GetOrdersAsync(OrderStatusEnum? status, int ?lastId = null,
            int? pageSize=10)
        {
            var query = _context.Orders.AsNoTracking()
                .Include(o => o.Delivery)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.PizzaSize)
                .Include(o => o.DelStatus)
                .OrderBy(o => o.Id).AsQueryable();
                //.Take(pageSize);
            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(o => o.DelStatusId == (int)status);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetActiveOrdersAsync(int? lastId = null,
            int? pageSize = 10)
        {
            var query = _context.Orders.AsNoTracking()
                .Include(o => o.Delivery)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.PizzaSize)
                .Include(o => o.DelStatus)
                .Include(o => o.Client)
                .OrderByDescending(o => o.Id)
                .Where(o => o.DelStatusId != (int)OrderStatusEnum.NotPlaced &&
                     o.DelStatusId != (int)OrderStatusEnum.IsCancelled

                     );
                //.Take(pageSize);
            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByClientIdAsync(string clientId, int? lastId=null, int? pageSize = 50)
        {
            var query = _context.Orders.AsNoTracking()
                .Include(o => o.Delivery)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.PizzaSize)
                .Include(o => o.DelStatus)
                .OrderByDescending(o => o.Id)
                .Where(o => o.ClientId == clientId && o.DelStatusId != (int)OrderStatusEnum.NotPlaced);
                //.Take(pageSize);
            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByCourierIdAsync(string courierId, OrderStatusEnum? status, int? lastId = null, int? pageSize = 50)
        {
            var query = _context.Orders.AsNoTracking()
                .Include(o => o.Delivery)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.PizzaSize)
                .Include(o => o.DelStatus)
                .OrderByDescending(o => o.Id)
                .Where(o => o.Delivery != null && o.Delivery.CourierId == courierId ||
                o.DelStatusId != (int)OrderStatusEnum.NotPlaced ||
                o.DelStatusId != (int)OrderStatusEnum.IsBeingFormed ||
                o.DelStatusId != (int)OrderStatusEnum.IsBeingPrepared);
                //.Take(pageSize);
            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(o => o.DelStatusId == (int)status);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId,
            int? lastId=null, int? pageSize=10)
        {
            var query = _context.Orders.AsNoTracking()
                .OrderByDescending(o=>o.Id)
                .Where(o => o.ClientId == userId);
                //.OrderBy(o => o.Id);
                //.Take(pageSize);

            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);

            return await query.ToListAsync();

            //return await _context.Orders.Where(o => o.ClientId == userId).Include(p=>p.OrderLines)
            //    .ThenInclude(ol=>ol.Pizza).ThenInclude(p=>p.Ingredients).Include(o=>o.Deliveries).ToListAsync();
        }

        public async Task<Order?> GetCartAsync(string clientId)
        {
            var query = _context.Orders
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Pizza)
                    .ThenInclude(p => p.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Ingredients)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.PizzaSize)
                .Where(o => o.ClientId == clientId && o.DelStatusId == (int)OrderStatusEnum.NotPlaced);
            return await query.FirstOrDefaultAsync();

        }

        public async Task AddOrderAsync(Order order)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            //try
            //{
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();
            //}
            //catch(Exception ex)
            //{
            //    await transaction.RollbackAsync();
                
            //}
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        

        public async Task CancelOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                //if (order.OrderLines.Any())
                //{
                //    throw new InvalidOperationException("Заказ не может быть удален, потому что он связан со строками.");
                //}

                //_context.Orders.Remove(order);

                order.DelStatusId = (int)OrderStatusEnum.IsCancelled;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order> GetOrderWithDeliveryAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersForCourier(string courierId, int? lastId, int? pageSize)
        {
            var query = _context.Orders.AsNoTracking()
               .Include(o => o.Delivery)
               .Include(o => o.OrderLines)
                   .ThenInclude(ol => ol.Pizza)
                   .ThenInclude(p => p.Ingredients)
               .Include(o => o.OrderLines)
                   .ThenInclude(ol => ol.Ingredients)
               .Include(o => o.OrderLines)
                   .ThenInclude(ol => ol.PizzaSize)
               .Include(o => o.DelStatus)
               .OrderByDescending(o => o.Id)
               .Where(o => o.DelStatusId == (int)OrderStatusEnum.IsBeingTransferred ||
               //o.DelStatusId==(int)OrderStatusEnum.IsNotDelivered ||
               //o.DelStatusId==(int)OrderStatusEnum.HasBeenTransferred ||
               //o.DelStatusId==(int)OrderStatusEnum.IsDelivered
               o.Delivery.CourierId == courierId);

            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(
            int statusId,
            int? lastId,
            int? pageSize)
        {
            var query = _context.Orders.AsNoTracking()
               .Include(o => o.Delivery)
               .Include(o => o.OrderLines)
                   .ThenInclude(ol => ol.Pizza)
                   .ThenInclude(p => p.Ingredients)
               .Include(o => o.OrderLines)
                   .ThenInclude(ol => ol.Ingredients)
               .Include(o => o.OrderLines)
                   .ThenInclude(ol => ol.PizzaSize)
               .Include(o => o.DelStatus)
               .OrderByDescending(o => o.Id)
               .Where(o => o.DelStatusId == statusId);
               
            if (lastId.HasValue)
            {
                query = query.Where(o => o.Id < lastId.Value);
            }
            if (pageSize.HasValue)
                query = query.Take(pageSize.Value);
            return await query.ToListAsync();
 
        }


    }


}
