using DesignPattern.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Repositories
{
    public class ProductReporitory : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();

        public void Add(Product item)
        {
            _products.Add(item);
        }

        public List<Product> GetAllProducts()
        {
            return _products;
        }
    }
}
