using Frontend.HttpsClients.Products;
using Frontend.Models.Products;
using Frontend.Services.Interfaces;

namespace Frontend.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger _logger;
        private readonly IProductApiClient _productApiClient;
        public ProductService(IProductApiClient productApiClient, ILogger<ProductService> logger)
        {
            _productApiClient = productApiClient;
            _logger = logger;
        }

        
        public async Task<(string Message, int StatusCode, IEnumerable<DTOs.ProductBuyerDTO?>)> GetProductByStoreIdAsync(Guid storeId)
        {
            var (success, message, statusCode, data) = await _productApiClient.GetByStoreAsync(storeId);
            if (!success)
            {
                return (message, statusCode, Enumerable.Empty<DTOs.ProductBuyerDTO?>());
            }
            return (message, statusCode, data);
        }   


        public async Task<(string Message, int StatusCode, IEnumerable<DTOs.CategoryDTO?>)> SearchCategoriesByStoreIdAsync(Guid storeId)
        {
            var (success, message, statusCode, data) =  await _productApiClient.SearchCategoriesAsync(storeId);
            if (!success)
            {
                return (message, statusCode, Enumerable.Empty<DTOs.CategoryDTO?>());
            }
            return (message, statusCode, data);
        }

        
    }
}

//public async Task<(string Message, int StatusCode, DTOs.ProductBuyerDTO?)> GetProductByIdAsync(Guid productId)
//{
//    var (success, message, statusCode, data) = await _productApiClient.GetByIdAsync(productId);
//    if (!success)
//    {
//        return (message, statusCode, null);
//    }
//    return (message, statusCode, data);
//}