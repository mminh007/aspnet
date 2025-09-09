using DAL.Databases;
using DAL.Models.Entities;
using DAL.Repository;
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

        public async Task AddProductAsync(ProductModel product)
        {
            await _db.Products.AddAsync(product);
        }

        public async Task<int> DeleteProductAsync(Guid productId)
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product == null)
            {
                return 0;
            }
            ;

            product.IsActive = false;

            return 1;

        }

        public async Task<ProductModel?> GetByIdAsync(Guid productId)
        {
            var product = await _db.Products.FirstOrDefaultAsync(
                x => x.ProductId == productId);

            return product;

        }



        public async Task<IEnumerable<ProductModel>> GetProductByStoreAngCategoryIdAsync(Guid storeId, Guid categoryId)
        {
            return await _db.Products
                            .Include(p => p.Category)
                            .Where(p => p.StoreId == storeId && p.CategoryId == categoryId)
                            .ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetProductByStoreIdAsync(Guid storeId)
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
                            .Where(p => p.ProductName.Contains(keyword) ||
                                        p.Description.Contains(keyword) ||
                                        p.Supplier.Contains(keyword))
                            .ToListAsync();
        }


        //public async Task<int> UpdateProductAsync(UpdateProductModel dto, Guid ProductId)
        //{
        //    var item = await _db.Products.FirstOrDefaultAsync(
        //                          p => p.ProductId == ProductId);
        //    if (item == null) return 0;

        //    if (dto.ProductName != null) item.ProductName =  dto.ProductName;
        //    if (dto.Description != null) item.Description = dto.Description;
        //    if (dto.Supplier != null) item.Supplier = dto.Supplier;
        //    if (dto.SalePrice.HasValue) item.SalePrice = dto.SalePrice.Value;
        //    if (dto.Quantity.HasValue) item.Quantity = dto.Quantity.Value;
        //    if (dto.IsActive.HasValue) item.IsActive = dto.IsActive.Value;
        //    if (dto.ImportPrice.HasValue) item.ImportPrice = dto.ImportPrice.Value;
        //    if (dto.CategoryId != null) item.CategoryId = dto.CategoryId.Value;

        //    item.UpdatedAt = DateTime.UtcNow;

        //    return 1;

        //}

        // Buyer

        public async Task<IEnumerable<ProductModel>> GetProductByCategoryAsync(Guid categoryId)
        {
            var productList = await _db.Products
                                       .Include(x => x.CategoryId)
                                       .Where(p => p.CategoryId == categoryId)
                                       .ToListAsync();
            return productList;


        }

        public async Task<IEnumerable<ProductModel>> SearchProductAsync(string keyword)
        {
            return await _db.Products
                            .Include(p => p.Category)
                            .Where(p => p.ProductName.Contains(keyword) ||
                                        p.Description.Contains(keyword) ||
                                        p.Supplier.Contains(keyword))
                            .ToListAsync();

        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }



        public async Task CreateCategoryAsync(CategoryModel model)
        {

            await _db.Category.AddAsync(model);


        }

        public async Task<IEnumerable<CategoryModel>> SearchCategoriesAsync(Guid storeId)
        {
            var itemList = await _db.Category
                                    .Where(c => c.StoreId == storeId)
                                    .Include(c => c.Products)
                                    .ToListAsync();
            return itemList;
        }


    }
}
