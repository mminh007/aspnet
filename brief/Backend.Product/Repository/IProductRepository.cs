using Backend.Product.Models.Entities;

namespace Backend.Product.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> GetProductByStoreIdAsync (Guid storeId);

        Task<IEnumerable<ProductModel>> GetProductByStoreAngCategoryIdAsync (Guid storeId, Guid categoryId);

        Task<IEnumerable<ProductModel>> SearchProductByStoreAsync (Guid storeId, string keyword);

        Task AddProductAsync(ProductModel product);

        //Task<int> UpdateProductAsync (UpdateProductModel updateProduct, Guid ProductId);

        Task<int> DeleteProductAsync (Guid productId);

        Task<IEnumerable<ProductModel>> GetProductByCategoryAsync (Guid categoryId);
        Task<IEnumerable<ProductModel>> SearchProductAsync(string keyword);

        Task <ProductModel?> GetByIdAsync(Guid productId);

        Task CreateCategoryAsync(CategoryModel model);
        Task<IEnumerable<CategoryModel>> SearchCategoriesAsync(Guid storeId);

        Task SaveChangesAsync();
    }
}
