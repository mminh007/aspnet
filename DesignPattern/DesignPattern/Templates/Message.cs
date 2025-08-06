using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Templates
{
    internal class Message : MessageTemplate
    {
        protected override void Body(string message)
        {
            Console.WriteLine($"Product's Name: {message}");
        }
    }
}
