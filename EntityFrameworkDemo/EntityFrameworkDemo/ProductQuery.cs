using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDemo
{
    public class ProductQuery
    {
        private readonly AppDbContext _context;

        public ProductQuery(AppDbContext context)
        {
            _context = context;
        }

        public List<Products> GetAllProducts()
        {
            return _context.Products.ToList();
        }

        public List<Products> GetPriceProduct(string name)
        {
            return _context.Products
                .Where(p => p.Name == name).ToList();
        }

        public List<Products> GetProductsByCategory(int categoryId)
        {
            return _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToList();
        }

        public List<Products> GetProductsWithDetails()
        {
            return _context.Products
                .Include(p => p.ProductDetail)
                .ToList();
        }

        public List<object> GetProductWithCategory()
        {
            return _context.Products
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    Category = p.Category.CategoryName

                }).ToList<object>();
        }

        public List<object> GetExpensiveProducts(int v)
        {
            return _context.Products
                .Where(p => p.Price > v)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    Category = p.Category.CategoryName
                }).ToList<object>();
        }

        public IEnumerable<object> GetFullProductInfo()
        {
            return _context.Products
                .Include(p => p.ProductDetail)
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    Category = p.Category.CategoryName,
                    Description = p.ProductDetail.Description,
                    Manufacturer = p.ProductDetail.Manufacturer
                });
        }
    }
}
