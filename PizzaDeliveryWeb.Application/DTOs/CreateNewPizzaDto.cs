using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PizzaDeliveryWeb.Application.DTOs
{
    //DTO на время, пока не будет реализована функция указания ингредиентов пиццы на стороне клиента
    public class CreateNewPizzaDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public IFormFile Image { get; set; }
    }
}
