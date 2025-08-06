using Microsoft.Extensions.DependencyInjection;
using DesignPattern.Repositories;
using DesignPattern.Products;
using DesignPattern.Templates;
using DesignPattern.Services;

namespace DesignPattern
{
    internal class Program
    {
        static void Main()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IProductRepository, ProductReporitory>();
            services.AddSingleton<MessageTemplate, Message>();
            services.AddSingleton<ProductService>();

            var serviceProvider = services.BuildServiceProvider();

            var productService = serviceProvider.GetRequiredService<ProductService>();
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1"},
                new Product { Id = 2, Name = "Product 2"},
                new Product { Id = 3, Name = "Product 3"}
            };

            foreach (var product in products)
            {
                productService.AddProduct(product);
            }

        }
    }
}
