using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Templates
{
    public abstract class MessageTemplate
    {
        public void ShowMessage(string message)
        {
            Console.WriteLine("Message Template Start");
            Body(message);

            Console.WriteLine("Message Template End");
            Console.WriteLine(new string('-', 30));
        }

        protected abstract void Body(string message);

    }
}
