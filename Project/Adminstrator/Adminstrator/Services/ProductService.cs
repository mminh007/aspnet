using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Products;
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
    }
}
