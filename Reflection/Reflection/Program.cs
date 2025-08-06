using System;
using System.Reflection;
using System.Reflection.Metadata;




namespace Reflection
{
    public class Program
    {
        static void Main(string[] args)
        {
            Type type = typeof(User);

            Console.WriteLine($"Type: {type.Name}");
            Console.WriteLine(new string('-', 30));

            // Get all fields, including private ones
            foreach (var item in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Console.WriteLine($"Field in User class: {item}");
                
            }

            // Get all methods, including private ones
            MethodInfo[] methods = type.GetMethods();

            foreach (var method in methods)
            {
                Console.WriteLine($"Method in User class: {method}");
                
            }
            Console.WriteLine(new string('-', 30));


            // Get all properties, including required ones
            foreach (var item in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Console.WriteLine($"Properties in User class: {item}");
            }
            Console.WriteLine(new string('-', 30));


            Console.WriteLine($"Get Method SayHello(): {type.GetMethod("SayHello")}");


            // Create an instance of User using Activator
            var user = Activator.CreateInstance(type);
            Console.WriteLine($"CreateInstance: {user}");
            Console.WriteLine();

            var field = type.GetField("_secret", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Console.WriteLine("Field '_secret' not found.");
                return;
            }
            Console.WriteLine($"Value of Field _secret: {field.GetValue(user)}");

            // Set new value for the private field _secret
            field.SetValue(user, "new-secret-value");
            Console.WriteLine($"New value of Field _secret: {field.GetValue(user)}");

            Console.WriteLine(new string('-', 30));


            var user2 = new User()
            {
                Name = "default name",
                Age = 25
            };

            Console.WriteLine(user2);

        }
    }
}
