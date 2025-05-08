using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PizzaDeliveryWeb.Application.MyExceptions
{
    public class NotFoundException : Exception
    {

        public string EntityType { get; }
        public object Key { get; }
        public NotFoundException(string entityType, object key)
       : base($"Сущность \"{entityType}\" с ключом \"{key}\" не найдена.")
        {
            EntityType = entityType;
            Key = key;
        }
        public NotFoundException(string message) : base(message) { }
    }
}
