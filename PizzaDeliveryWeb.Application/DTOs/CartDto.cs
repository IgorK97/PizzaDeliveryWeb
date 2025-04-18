using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string Address { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalPrice { get; set; }
            //=> Items.Sum(i => i.ItemPrice * i.Quantity);
        public decimal TotalWeight { get; set; }
            //=> Items.Sum(i => i.ItemWeight * i.Quantity);
    }
}
