using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Application.MyExceptions
{
    public class MyDbException : Exception
    {
        public MyDbException(string message, Exception inner) : base(message, inner) { }
    }
}
