using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class CreatePizzaDto
    {
        //public int Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public string Image { get; set; }
        //public IFormFile Image { get; set; }
        public List<int> DefaultIngredientIds { get; set; } = new List<int>();

    }
}
