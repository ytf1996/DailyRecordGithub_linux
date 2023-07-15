using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    public class LogicException : Exception
    {
        public LogicException(string message) : base(message)
        {

        }
    }
}