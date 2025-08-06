using DesignPattern.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.Repositories
{
    public interface IProductRepository

    {
        List<Product> GetAllProducts();
        void Add(Product product);
    }
}
