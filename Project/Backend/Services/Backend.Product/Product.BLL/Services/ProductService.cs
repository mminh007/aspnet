using AutoMapper;
using BLL.Services.Interfaces;
using Common.Enums;
using Common.Models;
using Common.Models.Requests;
using Common.Models.Responses;
using DAL.Models.Entities;
using DAL.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Common.Configs;
using Product.Common.Models.Requests;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IMapper _mapper;
        private readonly StaticFileConfig _staticFileConfig;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, IMapper mapper, IOptions<StaticFileConfig> staticFileConfig, ILogger<ProductService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _staticFileConfig = staticFileConfig.Value;
            _logger = logger;
        }

        // ---------------------------
        // Product
        // ---------------------------

        public async Task<ProductResponseModel<DTOs.ProductSellerDTO>> GetProductByIdAsync(Guid productId)
        {
            var product = await _repo.GetByIdAsync(productId);
            if (product == null)
            {
                return new ProductResponseModel<DTOs.ProductSellerDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound
                };
            }

            var dto = _mapper.Map<DTOs.ProductSellerDTO>(product);

            dto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{product.ProductImage}";

            return new ProductResponseModel<DTOs.ProductSellerDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }

        public async Task<ProductResponseModel<IEnumerable<object>>> GetProductsByStoreAsync(Guid storeId, string userRole)
        {
            var products = await _repo.GetProductsByStoreIdAsync(storeId);


            if (userRole == "seller")
            {
                var dtos = _mapper.Map<IEnumerable<DTOs.ProductSellerDTO>>(products);

                foreach (var dto in dtos)
                {
                    dto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{dto.ProductImage}";
                    _logger.LogInformation("Product Image URL: {ProductImageUrl}", dto.ProductImage);
                }

                return new ProductResponseModel<IEnumerable<object>>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dtos.Cast<object>().ToList()
                };
            }
            else
            {
                var dtos = _mapper.Map<IEnumerable<DTOs.ProductBuyerDTO>>(products);

                foreach (var dto in dtos)
                {
                    dto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{dto.ProductImage}";
                }

                return new ProductResponseModel<IEnumerable<object>>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dtos.Cast<object>().ToList()
                };
            }
        }

        public async Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> GetProductsByStoreAndCategoryAsync(Guid storeId, Guid categoryId)
        {
            var products = await _repo.GetProductByStoreAndCategoryIdAsync(storeId, categoryId);
            var dtos = _mapper.Map<IEnumerable<DTOs.ProductSellerDTO>>(products);

            foreach (var dto in dtos)
            {
                dto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{dto.ProductImage}";
                _logger.LogInformation("Product Image URL: {ProductImageUrl}", dto.ProductImage);
            }

            return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dtos
            };
        }

        public async Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> SearchProductsByStoreAsync(Guid storeId, string keyword)
        {
            var products = await _repo.SearchProductByStoreAsync(storeId, keyword);
            var dtos = _mapper.Map<IEnumerable<DTOs.ProductSellerDTO>>(products);

            foreach (var dto in dtos)
            {
                dto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{dto.ProductImage}";
            }

            return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dtos
            };
        }

        public async Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> CreateProductAsync(DTOs.ProductDTO dto)
        {
            try
            {
                var entity = _mapper.Map<ProductModel>(dto);
                var created = await _repo.AddProductAsync(entity);
                await _repo.SaveChangesAsync();

                //var prodDto = _mapper.Map<DTOs.ProductSellerDTO>(created);

                //prodDto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{prodDto.ProductImage}";

                var products = await _repo.GetProductsByStoreIdAsync(dto.StoreId);

                var productList = _mapper.Map<IEnumerable<DTOs.ProductSellerDTO>>(products);


                return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = productList,
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> UpdateProductAsync(UpdateProductModel dto)
        {
            try
            {
                
                var product = _mapper.Map<ProductModel>(dto);

                if(dto.ProductImage != null)
                {
                    product.ProductImage = dto.ProductImage;
                }

                var newProduct = await _repo.UpdateProductAsync(product, product.ProductId);

                if (newProduct != null)
                {
                    var products = await _repo.GetProductsByStoreIdAsync(newProduct.StoreId);

                    var productList = _mapper.Map<IEnumerable<DTOs.ProductSellerDTO>>(products);

                    return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
                    {
                        Success = true,
                        Data = productList,
                    };
                }

                return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
                {
                    Success = false,

                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel<string>> DeleteProductAsync(Guid productId)
        {
            try
            {
                var result = await _repo.DeleteProductAsync(productId);
                if (result == 0)
                {
                    return new ProductResponseModel<string>
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Product not found"
                    };
                }

                return new ProductResponseModel<string>
                {
                    Success = true,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel<string>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel<object>> ChangeActiveProductAsync(ChangeActiveProduct request)
        {
            var result = await _repo.UpdateActiveProduct(request.ProductId, request.IsActive);

            if (result == 0)
            {
                return new ProductResponseModel<object>
                {
                    Success = false,
                    ErrorMessage = "Product not found!"
                };
            }

            return new ProductResponseModel<object>
            {
                Success = true,

            };
        }


        //---------------------------
        // Category
        //---------------------------
        public async Task<ProductResponseModel<DTOs.CategoryDTO>> CreateCategoryAsync(DTOs.CategoryDTO category)
        {
            try
            {
                var entity = _mapper.Map<CategoryModel>(category);
                var created = await _repo.CreateCategoryAsync(entity);
                await _repo.SaveChangesAsync();

                var dto = _mapper.Map<DTOs.CategoryDTO>(created);

                return new ProductResponseModel<DTOs.CategoryDTO>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel<DTOs.CategoryDTO>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel<IEnumerable<DTOs.CategoryDTO>>> SearchCategoriesAsync(Guid storeId)
        {
            var categories = await _repo.SearchCategoriesAsync(storeId);
            var dtos = _mapper.Map<IEnumerable<DTOs.CategoryDTO>>(categories);

            return new ProductResponseModel<IEnumerable<DTOs.CategoryDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dtos
            };
        }

        public async Task<ProductResponseModel<int>> DeleteCategoryAsync(Guid categoryId)
        {
            var result = await _repo.DeleteCategoryAsync(categoryId);

            if (result == -1)
            {
                return new ProductResponseModel<int>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Any Product belong to category"
                };
            }

            else if (result == 0) {
                return new ProductResponseModel<int>
                {
                    Success = false,
                    Message = OperationResult.Failed,
                    ErrorMessage = "Category not found"
                };
            }

            return new ProductResponseModel<int>
            {
                Success = true,
                Message = OperationResult.Success,
            };

        }


        //---------------------------
        // Order Product Info
        //---------------------------
        public async Task<ProductResponseModel<IEnumerable<DTOs.OrderProductDTO>>> OrderGetProductInfo2(List<Guid> productIds)
        {
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    return new ProductResponseModel<IEnumerable<DTOs.OrderProductDTO>>
                    {
                        Success = false,
                        Message = OperationResult.Failed,
                        ErrorMessage = "ProductIds list is empty"
                    };
                }

                // Lấy toàn bộ sản phẩm trong list
                var products = new List<ProductModel>();
                var notFound = new List<Guid>();

                foreach (var id in productIds)
                {
                    var product = await _repo.GetByIdAsync(id);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                    else
                    {
                        notFound.Add(id);
                    }
                }

                if (!products.Any())
                {
                    return new ProductResponseModel<IEnumerable<DTOs.OrderProductDTO>>
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "No products found",
                        NotFoundProductIds = notFound
                    };
                }

                // Map sang DTO
                var dtos = _mapper.Map<IEnumerable<DTOs.OrderProductDTO>>(products);

                foreach (var d in dtos)
                {
                    d.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{d.ProductImage}";
                }
                
                return new ProductResponseModel<IEnumerable<DTOs.OrderProductDTO>>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dtos,
                    NotFoundProductIds = notFound.Any() ? notFound : null
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel<IEnumerable<DTOs.OrderProductDTO>>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<ProductResponseModel<DTOs.OrderProductDTO>> OrderGetProductInfo(Guid productId)
        {
            var product = await _repo.GetByIdAsync(productId);
            if (product == null)
            {
                return new ProductResponseModel<DTOs.OrderProductDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound
                };
            }

            var dto = _mapper.Map<DTOs.OrderProductDTO>(product);

            dto.ProductImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{dto.ProductImage}";

            return new ProductResponseModel<DTOs.OrderProductDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }

    }
}
