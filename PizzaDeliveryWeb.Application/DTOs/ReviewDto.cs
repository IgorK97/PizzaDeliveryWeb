using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaDeliveryWeb.Domain.Entities;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class ReviewDto
    {
        
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        public string ClientId { get; set; }
        public int Rating { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Order Order { get; set; }
        public User Client { get; set; }
    }
}
