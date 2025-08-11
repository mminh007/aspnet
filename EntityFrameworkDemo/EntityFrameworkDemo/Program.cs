
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EntityFrameworkDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


            //RunApp(context);
            var query = new ProductQuery(context);

            var allProducts = query.GetAllProducts();
            Console.WriteLine();
            Console.WriteLine($"Total Products: {allProducts.Count}");

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            var expensive = query.GetExpensiveProducts(50);
            Console.WriteLine();
            Console.WriteLine($"Price > 50: {expensive.Count}");
            foreach (var p in expensive)
            {
                Console.WriteLine($"- {p}");
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            var fullInfo = query.GetFullProductInfo();
            Console.WriteLine();
            foreach (var p in fullInfo)
            {
                Console.WriteLine($"- {p}");
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            var productWithCategory = query.GetProductWithCategory();
            Console.WriteLine();
            foreach (var p in productWithCategory)
            {
                Console.WriteLine($"- {p}");
            }
        }

        


        //private static void RunApp(AppDbContext context)
        //{
        //    var random = new Random();


        //    if (!context.Categories.Any())
        //    {
        //        var categories = new List<Category>
        //        {
        //            new Category { CategoryName = "Electronics" },
        //            new Category { CategoryName = "Books" },
        //            new Category { CategoryName = "Clothing" }
        //        };

        //        context.Categories.AddRange(categories);
        //        context.SaveChanges();
        //    }

        //    var allCategories = context.Categories.ToList();


        //    for (int i = 1; i <= 10; i++)
        //    {
        //        int randomId = random.Next(1, 1000);
        //        decimal randomPrice = random.Next(10, 101);
        //        var category = allCategories[random.Next(allCategories.Count)];

        //        var product = new Products
        //        {
        //            Name = $"Sample Product {randomId}",
        //            Price = randomPrice,
        //            CategoryId = category.CategoryId,
        //            ProductDetail = new ProductDetail
        //            {
        //                Description = $"This is a description for product {randomId}",
        //                Manufacturer = $"Manufacturer {random.Next(1, 5)}"
        //            }
        //        };

        //        context.Products.Add(product);
        //    }

        //    context.SaveChanges();

        //    var queriedProduct = context.Products
        //        .Include(p => p.Category)
        //        .Include(p => p.ProductDetail)
        //        .FirstOrDefault();

        //    if (queriedProduct != null)
        //    {
        //        Console.WriteLine($"✅ Product found: {queriedProduct.Name}");
        //        Console.WriteLine($"   - Price: {queriedProduct.Price}");
        //        Console.WriteLine($"   - Category: {queriedProduct.Category?.CategoryName}");
        //        Console.WriteLine($"   - Description: {queriedProduct.ProductDetail?.Description}");
        //        Console.WriteLine($"   - Manufacturer: {queriedProduct.ProductDetail?.Manufacturer}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("❌ No product found.");
        //    }
        //}
    }
}
