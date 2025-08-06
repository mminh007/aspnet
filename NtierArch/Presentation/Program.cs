using BLL;
using DAL;
using Models;

namespace Presentation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var repository = new Repository();
            var service = new Service(repository);

            // Adding a user
            service.AddUser(new Users { Id = 1, Name = "John Doe" });
            service.AddUser(new Users { Id = 2, Name = "Jane Smith" });
            service.AddUser(new Users { Id = 3, Name = "Alice Johnson" });

            Console.WriteLine("All Users:");
            var users = service.GetAllUsers();
            foreach (var user in users)
            {
                Console.WriteLine($"Id: {user.Id}, Name: {user.Name}");
            }
            Console.WriteLine();

           
            int length = users.Count;

            Console.Write("Enter user ID to retrieve: ");
            int userId = int.Parse(Console.ReadLine());

            while (userId < 1 || userId > length)
            {
                Console.WriteLine($"Invalid user ID. Please enter integer in range 1 - {length}.");
                Console.WriteLine("Please enter user ID again.");
                int.TryParse(Console.ReadLine(), out userId);
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            // Retrieving a user by ID
            var userById = service.GetUserById(userId);
            Console.WriteLine($"User found: Id: {userById.Id}, Name: {userById.Name}");




        }
    }
}
