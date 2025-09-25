using Common.Models.Requests;
using DAL.Databases;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _db;

        public ProductRepository(ProductDbContext db)
        {
            _db = db;
        }

        public async Task<ProductModel> AddProductAsync(ProductModel product)
        {
            product.ProductId = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.Products.AddAsync(product);
            return product;
        }

        public async Task<int> DeleteProductAsync(Guid productId)
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (product == null) return 0;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            return await _db.SaveChangesAsync();
        }

        public async Task<ProductModel?> GetByIdAsync(Guid productId)
        {
            return await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<ProductModel>> GetProductByStoreAndCategoryIdAsync(Guid storeId, Guid categoryId)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => p.StoreId == storeId && p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetProductsByStoreIdAsync(Guid storeId)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => p.StoreId == storeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> SearchProductByStoreAsync(Guid storeId, string keyword)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => p.StoreId == storeId &&
                           ((p.ProductName ?? "").Contains(keyword) ||
                            (p.Description ?? "").Contains(keyword) ||
                            (p.Supplier ?? "").Contains(keyword)))
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetProductByCategoryAsync(Guid categoryId)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> SearchProductAsync(string keyword)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => (p.ProductName ?? "").Contains(keyword) ||
                            (p.Description ?? "").Contains(keyword) ||
                            (p.Supplier ?? "").Contains(keyword))
                .ToListAsync();
        }

        public async Task<CategoryModel> CreateCategoryAsync(CategoryModel category)
        {
            category.CategoryId = Guid.NewGuid();
            await _db.Category.AddAsync(category);
            return category;
        }

        public async Task<IEnumerable<CategoryModel>> SearchCategoriesAsync(Guid storeId)
        {
            return await _db.Category
                .Include(c => c.Products)
                .Where(c => c.StoreId == storeId)
                .ToListAsync();
        }

        public async Task<ProductModel?> UpdateProductAsync(ProductModel updateProduct, Guid productId)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null) return null;

            // Nếu có SalePrice mới và khác với DB → gọi hàm UpdateProductPriceAsync
            if (updateProduct.SalePrice != product.SalePrice)
            {
                await UpdateProductPriceAsync(productId, updateProduct.SalePrice);
            }

            // Cập nhật các field khác
            product.ProductName = updateProduct.ProductName;
            product.Description = updateProduct.Description;
            product.ImportPrice = updateProduct.ImportPrice;
            product.ProductImage = updateProduct.ProductImage;
            product.Quantity = updateProduct.Quantity;
            product.Supplier = updateProduct.Supplier;
            product.CategoryId = updateProduct.CategoryId;
            product.IsActive = updateProduct.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<int> UpdateProductPriceAsync(Guid productId, decimal newPrice)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null) return 0;

            var oldPrice = product.SalePrice;
            product.SalePrice = newPrice;
            product.UpdatedAt = DateTime.UtcNow;

            // TODO: phát integration event khi giá thay đổi
            // var integrationEvent = new IntegrationEventLog { ... };
            // await _db.IntegrationEventLogs.AddAsync(integrationEvent);

            return await _db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}




