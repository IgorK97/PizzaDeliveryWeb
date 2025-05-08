using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.MyExceptions
{
    public class CartNotFoundException : Exception
    {
        public CartNotFoundException(string message) : base(message) { }
    }
}
