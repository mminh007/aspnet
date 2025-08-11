using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DBFristDemo.Database;
using DBFristDemo.Services;
using DBFristDemo.Models;
using DBFristDemo.Repository;

namespace DBFristDemo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register DbContext with connection string from appsettings.json
                    var cs = context.Configuration.GetConnectionString("MyDB");
                    services.AddDbContext<MyDbContext>(options =>
                        options.UseSqlServer(cs));
                    services.AddScoped<IStaffRepository, StaffRepository>();
                    
                    // Register other services
                    services.AddScoped<IMyService, MyService>();
                })
                .Build();

            // Run the application
            using var scope = host.Services.CreateScope();
            var myService = scope.ServiceProvider.GetRequiredService<IMyService>();

            // var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            Console.WriteLine("DB First Demo Application");
            Console.WriteLine();

            Console.WriteLine("List of Employees");
            Console.WriteLine();
            List<Employee> employees = await myService.GetAllAsync(includeNavigation: false);
            foreach (var employee in employees)
            {
                Console.WriteLine($"ID: {employee.EmployeeId}, Name: {employee.FirstName} {employee.LastName}, Department: {employee.Department?.DepartmentName ?? "N/A"}, ID_Manager: {employee.ManagerId}");
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            Console.WriteLine("List of Employees By ManagerId");
            Console.WriteLine("Enter Manager ID:");
            int managerId = int.Parse(Console.ReadLine() ?? "0");

            List<Employee> employeesByManager = await myService.GetEmployeesByManagerIdAsync(managerId, includeNavigation: false);
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            foreach (var employee in employeesByManager)
            {
                Console.WriteLine($"ID: {employee.EmployeeId}, Name: {employee.FirstName} {employee.LastName}, Department: {employee.Department?.DepartmentName ?? "N/A"}, ID_Manager: {employee.ManagerId}");
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            Console.WriteLine("Entered Employee ID: ");
            int EmployeeId = int.Parse(Console.ReadLine() ?? "0");

            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            Employee employeeById = await myService.GetByIdAsync(EmployeeId, includeNavigation: true);
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            Console.WriteLine($"ID: {employeeById.EmployeeId}, Name: {employeeById.FirstName} {employeeById.LastName}, " +
                                                            $"Department: {employeeById.Department?.DepartmentName ?? "N/A"}, " +
                                                            $"Job: {employeeById.Job.JobTitle}, " +
                                                            $"Manager: {employeeById.Manager?.FirstName} {employeeById.Manager?.LastName}");

        }
    }
}
