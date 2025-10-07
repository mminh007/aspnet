using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Products;
using Adminstrator.Models.Products.Requests;
using Adminstrator.Services.Interfaces;

namespace Adminstrator.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductApiClient _productApiClient;

        public ProductService(IProductApiClient productApiClient)
        {
            _productApiClient = productApiClient;
        }

        // ---------------------------
        // Get all products by storeId
        // ---------------------------
        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> GetByStoreAsync(Guid storeId)
        {
            return await _productApiClient.GetByStoreAsync(storeId);
        }

        // ---------------------------
        // Get product by Id
        // ---------------------------
        public async Task<(bool Success, string? Message, int statusCode, DTOs.ProductSellerDTO? Data)> GetByIdAsync(Guid productId)
        {
            return await _productApiClient.GetByIdAsync(productId);
        }

        // ---------------------------
        // Search categories by storeId
        // ---------------------------
        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)> SearchCategoriesAsync(Guid storeId)
        {
            return await _productApiClient.SearchCategoriesAsync(storeId);
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> Update(Guid productId, UpdateProductModel model)
        {
            return await _productApiClient.UpdateProductAsync(productId, model);
        }

        public async Task<(bool Success, string? Message, int statusCode)> Create(DTOs.ProductDTO model)
        {
            return await _productApiClient.CreateProductAsync(model);
        }

        public async Task<(bool Success, string? Message, int statusCode, string Data)> Delete(Guid productId)
        {
            return await _productApiClient.DeleteProductAsync(productId);
        }

        public async Task<(bool Success, string? Message, int statusCode)> ChangeActiveProduct(ChangeActiveProduct request)
        {
            return await _productApiClient.UpdateActiveAsync(request);
        }

        public async Task<(bool Success, string? Message, int statucCode)> CreateCategoryAsync(DTOs.CategoryDTO category)
        {
            return await _productApiClient.CreateCategoryAsync(category);
        }

        public async Task<(bool Success, string? Message, int statusCode)> DeleteCategoryAsync(Guid categoryId)
        {
            return await _productApiClient.DeleteCategoryAsync(categoryId);
        }
    }
}
