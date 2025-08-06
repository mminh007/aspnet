using DesignPattern.Products;
using DesignPattern.Repositories;
using DesignPattern.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly MessageTemplate _message;

        public ProductService(IProductRepository productRepository, MessageTemplate msg)
        {
            _productRepository = productRepository;
            _message = msg;
        }
        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }
        public void AddProduct(Product product)
        {
            _message.ShowMessage(product.Name);
            _productRepository.Add(product);
        }
    }
}
