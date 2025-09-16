using AutoMapper;
using Common.Enums;
using Common.Models;
using Common.Models.Requests;
using Common.Models.Responses;
using DAL.Models.Entities;
using DAL.Repository;
using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
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

            return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dtos
            };
        }

        public async Task<ProductResponseModel<DTOs.ProductSellerDTO>> CreateProductAsync(DTOs.ProductDTO dto)
        {
            try
            {
                var entity = _mapper.Map<ProductModel>(dto);
                var created = await _repo.AddProductAsync(entity);
                await _repo.SaveChangesAsync();

                var prodDto = _mapper.Map<DTOs.ProductSellerDTO>(created);

                return new ProductResponseModel<DTOs.ProductSellerDTO>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = prodDto
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel<DTOs.ProductSellerDTO>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> UpdateProductAsync(IEnumerable<UpdateProductModel> dtoList)
        {
            try
            {
                var updatedList = new List<DTOs.ProductSellerDTO>();
                var notFound = new List<Guid>();

                foreach (var dto in dtoList)
                {
                    var entity = _mapper.Map<ProductModel>(dto);
                    entity.ProductId = dto.ProductId;

                    var updated = await _repo.UpdateProductAsync(entity, entity.ProductId);
                    if (updated != null)
                    {
                        updatedList.Add(_mapper.Map<DTOs.ProductSellerDTO>(updated));
                    }
                    else
                    {
                        notFound.Add(dto.ProductId);
                    }
                }

                return new ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>
                {
                    Success = updatedList.Any(),
                    Message = updatedList.Any() ? OperationResult.Success : OperationResult.NotFound,
                    Data = updatedList,
                    NotFoundProductIds = notFound
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

        //---------------------------
        // Order Product Info
        //---------------------------
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

            return new ProductResponseModel<DTOs.OrderProductDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }
    }
}
