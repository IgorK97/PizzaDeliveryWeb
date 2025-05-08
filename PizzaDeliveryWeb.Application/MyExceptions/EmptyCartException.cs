using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.MyExceptions
{
    public class EmptyCartException : Exception
    {
        public EmptyCartException(string message) : base(message) { }
    }
}
