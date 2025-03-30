using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class ShortOrderDto
    {
        
        public int Id { get; set; }
        public string ClientId { get; set; }
       
        public string Address { get; set; }


    }
}
