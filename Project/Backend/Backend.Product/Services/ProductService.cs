using AutoMapper;
using Backend.Product.Models;
using Backend.Product.Repository;
using Backend.Shared.DTO.Products;
using Backend.Shared.Enums;
using System.Security.Claims;

namespace Backend.Product.Services
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
        // Seller
        // ---------------------------
        public async Task<ProductResponseModel> GetProductByIdAsync(Guid productId)
        {
            var product = await _repo.GetByIdAsync(productId);
            if (product == null)
            {
                return new ProductResponseModel
                {
                    Success = false,
                    Message = OperationResult.NotFound
                };
            }

            return new ProductResponseModel
            {
                Success = true,
                Message = OperationResult.Success,
                SellerProduct = new ProductSellerDTO
                {
                    ProductId = product.ProductId,
                    StoreId = product.StoreId,
                    CategoryId = product.CategoryId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    SalePrice = product.SalePrice,
                    ImportPrice = product.ImportPrice,
                    Quantity = product.Quantity,
                    Supplier = product.Supplier,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                }
            };
        }

        public async Task<ProductResponseModel> GetProductsByStoreAsync(Guid storeId, ClaimsPrincipal user)
        {
            var products = await _repo.GetProductByStoreIdAsync(storeId);

            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            if (roleClaim == "seller")
            {
                return new ProductResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                    SellerProductList = products.Select(p => new ProductSellerDTO
                    {
                        ProductId = p.ProductId,
                        StoreId = p.StoreId,
                        CategoryId = p.CategoryId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        SalePrice = p.SalePrice,
                        ImportPrice = p.ImportPrice,
                        Quantity = p.Quantity,
                        Supplier = p.Supplier,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                };
            }

            return new ProductResponseModel
            {
                Success = true,
                Message = OperationResult.Success,
                BuyerProductList = products.Select(p => new ProductBuyerDTO
                {
                    ProductName = p.ProductName,
                    Description = p.Description,
                    SalePrice = p.SalePrice,
                    Quantity = p.Quantity,
                    IsActive = p.IsActive
                })
            };
            
        }

        public async Task<ProductResponseModel> GetProductsByStoreAndCategoryAsync(Guid storeId, Guid categoryId)
        {
            var products = await _repo.GetProductByStoreAngCategoryIdAsync(storeId, categoryId);

            return new ProductResponseModel
            {
                Success = true,
                Message = OperationResult.Success,
                SellerProductList = products.Select(p => new ProductSellerDTO
                {
                    ProductId = p.ProductId,
                    StoreId = p.StoreId,
                    CategoryId = p.CategoryId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    SalePrice = p.SalePrice,
                    ImportPrice = p.ImportPrice,
                    Quantity = p.Quantity,
                    Supplier = p.Supplier,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
            };
        }

        public async Task<ProductResponseModel> SearchProductsByStoreAsync(Guid storeId, string keyword)
        {
            var products = await _repo.SearchProductByStoreAsync(storeId, keyword);

            return new ProductResponseModel
            {
                Success = true,
                Message = OperationResult.Success,
                SellerProductList = products.Select(p => new ProductSellerDTO
                {
                    ProductId = p.ProductId,
                    StoreId = p.StoreId,
                    CategoryId = p.CategoryId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    SalePrice = p.SalePrice,
                    ImportPrice = p.ImportPrice,
                    Quantity = p.Quantity,
                    Supplier = p.Supplier,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
            };
        }

        public async Task<ProductResponseModel> CreateProductAsync(ProductDTOModel dto)
        {
            var product = _mapper.Map<ProductModel>(dto);
            product.ProductId = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.AddProductAsync(product);
                await _repo.SaveChangesAsync();

                return new ProductResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel> UpdateProductAsync(IEnumerable<UpdateProductModel> dtoList)
        {
            try
            {
                int updatedCount = 0;
                var notFoundProducts = new List<Guid>();

                foreach (var dto in dtoList)
                {
                    if (dto.ProductId == Guid.Empty) continue;

                    var item = await _repo.GetByIdAsync(dto.ProductId);
                    if (item == null)
                    {
                        notFoundProducts.Add(dto.ProductId);
                        continue;
                    }

                    _mapper.Map(dto, item); 
                    item.UpdatedAt = DateTime.UtcNow;

                    updatedCount++;
                }

                if (updatedCount == 0 && notFoundProducts.Count > 0)
                {
                    return new ProductResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "No products were updated.",
                        NotFoundProductIds = notFoundProducts
                    };
                }

                await _repo.SaveChangesAsync();

                return new ProductResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                    NotFoundProductIds = notFoundProducts
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<ProductResponseModel> DeleteProductAsync(Guid productId)
        {
            try
            {
                var result = await _repo.DeleteProductAsync(productId);
                if (result == 0)
                {
                    return new ProductResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound
                    };
                }

                await _repo.SaveChangesAsync();
                return new ProductResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }


         //---------------------------
         //Category
         //---------------------------
        public async Task<ProductResponseModel> CreateCategoryAsync(CategoryDTO category)
        {
            try
            {
                var newCategory = new CategoryModel()
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = category.CategoryName,
                    StoreId = category.StoreId,

                };

                await _repo.CreateCategoryAsync(newCategory);
                await _repo.SaveChangesAsync();

                return new ProductResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                    CategoryInfo = new CategoryDTO
                    {
                        CategoryId = category.CategoryId,
                        StoreId = category.StoreId,
                        CategoryName = category.CategoryName
                    }
                };
            }
            catch (Exception ex)
            {
                return new ProductResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ProductResponseModel> SearchCategoriesAsync(Guid storeId)
        {
            var categories = await _repo.SearchCategoriesAsync(storeId);

            return new ProductResponseModel
            {
                Success = true,
                Message = OperationResult.Success,
                CategoryList = categories.Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    StoreId = c.StoreId,
                    CategoryName = c.CategoryName
                })
            };
        }


        // ---------------------------
        // Buyer
        // ---------------------------
        //public async Task<ProductResponseModel> GetProductsByCategoryForBuyerAsync(Guid categoryId)
        //{
        //    try
        //    {
        //        var products = await _repo.GetProductByCategoryAsync(categoryId);

        //        return new ProductResponseModel
        //        {
        //            Success = true,
        //            Message = OperationResult.Success,
        //            BuyerProductList = products.Select(p => new ProductBuyerDTO
        //            {
        //                ProductName = p.ProductName,
        //                Description = p.Description,
        //                SalePrice = p.SalePrice,
        //                Quantity = p.Quantity,
        //                IsActive = p.IsActive
        //            })
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ProductResponseModel
        //        {
        //            Success = false,
        //            Message = OperationResult.Error,
        //            ErrorMessage = ex.Message
        //        };
        //    }
        //}

        //public async Task<ProductResponseModel> SearchProductsForBuyerAsync(string keyword)
        //{
        //    try
        //    {
        //        var products = await _repo.SearchProductAsync(keyword);

        //        return new ProductResponseModel
        //        {
        //            Success = true,
        //            Message = OperationResult.Success,
        //            BuyerProductList = products.Select(p => new ProductBuyerDTO
        //            {
        //                ProductName = p.ProductName,
        //                Description = p.Description,
        //                SalePrice = p.SalePrice,
        //                Quantity = p.Quantity,
        //                IsActive = p.IsActive
        //            })
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ProductResponseModel
        //        {
        //            Success = false,
        //            Message = OperationResult.Error,
        //            ErrorMessage = ex.Message
        //        };
        //    }
        //}




    }
}
