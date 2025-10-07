using Adminstrator.Models.Products;
using Adminstrator.Models.Products.Requests;

namespace Adminstrator.Services.Interfaces
{
    public interface IProductService
    {
        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> GetByStoreAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, DTOs.ProductSellerDTO? Data)> GetByIdAsync(Guid productId);

        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> Update(Guid productId, UpdateProductModel model);

        Task<(bool Success, string? Message, int statusCode)> Create(DTOs.ProductDTO model);

        Task<(bool Success, string? Message, int statusCode, string Data)> Delete(Guid productId);

        Task<(bool Success, string? Message, int statusCode)> ChangeActiveProduct(ChangeActiveProduct request);


        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)> SearchCategoriesAsync(Guid storeId);

        Task<(bool Success, string? Message, int statucCode)> CreateCategoryAsync(DTOs.CategoryDTO category);
        Task<(bool Success, string? Message, int statusCode)> DeleteCategoryAsync(Guid categoryId);
    }


}
