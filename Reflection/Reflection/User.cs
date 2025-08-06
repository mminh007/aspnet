using System;
using System.ComponentModel.DataAnnotations;

namespace Reflection
{
    
    public class User
    {
        
        private string _secret = "default-secret";

        
        [Required]
        public required string Name { get; set; }

        public int Age { get; set; } = 18;

        
        public User() { }

        public User(string name, int age)
        {
            Name = name;
            Age = age;
        }

        
        public void SayHello()
        {
            Console.WriteLine($"Hello, {Name}!");
        }

        public void SetSecret(string secret)
        {
            _secret = secret;
        }

        public string GetSecret()
        {
            return _secret;
        }


        public static void StaticMethod()
        {
            Console.WriteLine("Called static method!");
        }


        private void PrivateMethod()
        {
            Console.WriteLine("This is a private method.");
        }

        public override string ToString()
        {
            return $"User(Name={Name}, Age={Age}, Secret={_secret})";
        }
    }
}
