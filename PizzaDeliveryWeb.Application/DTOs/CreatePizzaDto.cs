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
        [Key]
        public int Id { get; set; } 
        [Required]
        [StringLength(100, ErrorMessage = "Pizza Name cannot exceed 100 characters")] 
        public string Name { get; set; }
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public string Image { get; set; }
        //public IFormFile Image { get; set; }
        public List<int> Ingredients { get; set; } = new List<int>();

    }
}
