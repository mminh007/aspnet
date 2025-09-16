using DAL.Models.Entities;
using Common.Models.Requests;
using Common.Models;

namespace DAL.Repository
{
    public interface IProductRepository
    {
        // Search & Get
        Task<IEnumerable<ProductModel>> GetProductsByStoreIdAsync(Guid storeId); //userRole (seller or buyer)

        Task<IEnumerable<ProductModel>> GetProductByStoreAndCategoryIdAsync(Guid storeId, Guid categoryId);

        Task<IEnumerable<ProductModel>> SearchProductByStoreAsync(Guid storeId, string keyword);

        Task<IEnumerable<ProductModel>> GetProductByCategoryAsync(Guid categoryId);
        Task<IEnumerable<ProductModel>> SearchProductAsync(string keyword);

        Task<IEnumerable<CategoryModel>> SearchCategoriesAsync(Guid storeId);

        Task<ProductModel?> GetByIdAsync(Guid productId);


        // Create, Update, Delete 
        Task<ProductModel> AddProductAsync(ProductModel product);

        Task<ProductModel?> UpdateProductAsync (ProductModel updateProduct, Guid ProductId);
        Task<int> UpdateProductPriceAsync(Guid productId, decimal newPrice);

        Task<int> DeleteProductAsync(Guid productId);

        Task<CategoryModel> CreateCategoryAsync(CategoryModel model);


        Task SaveChangesAsync();
    }
}
