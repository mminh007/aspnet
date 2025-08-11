using Entities;
using Entities.User;
using Infrastructure;
using Application;

namespace Presentation

{
    internal class Program
    {
        static void Main(string[] args)
        {
            var repo = new Repository();
            var service = new UserService(repo);

            // Adding a user
            service.AddUser(new Users { Id = 1, Name = "John Doe" });
            service.AddUser(new Users { Id = 2, Name = "Jane Smith" });
            service.AddUser(new Users { Id = 3, Name = "Alice Johnson" });

            // Retrieving all users
            Console.WriteLine("All Users:");
            foreach (var user in service.GetAllUsers())
            {
                Console.WriteLine($"User ID: {user.Id}, Name: {user.Name}");
            }

            Console.WriteLine(new string('-', 50));

            // Retrieving a user by ID
            int length = service.GetAllUsers().Count;

            Console.WriteLine("Enter user ID to retrieve: ");
            int userId = int.Parse(Console.ReadLine());
            
            while (userId < 1 || userId > length)
            {
                Console.WriteLine($"Please enter a valid user ID between 1 and {length}: ");
                Console.WriteLine("Enter user ID: ");
                int.TryParse(Console.ReadLine(), out userId);
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("Retrieving user by ID...");

            var usrerById = service.GetUserById(userId);
            Console.WriteLine($"User ID: {usrerById?.Id}, Name: {usrerById?.Name ?? "User not found"}");

        }
    }
}
