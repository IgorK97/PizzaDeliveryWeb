using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.DTOs
{
    public class IngredientDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Ingredient Name cannot exceed 100 characters")] 
        public string Name { get; set; }
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        [Required]
        public decimal Small { get; set; }
        [Required]
        public decimal Medium { get; set; }
        [Required]
        public decimal Big { get; set; }
        [Required]
        public decimal PricePerGram { get; set; }
    }
}
