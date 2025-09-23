using AutoMapper;
using Microsoft.Extensions.Logging;
using Order.BLL.External.Interfaces;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using Order.DAL.Models.Entities;
using Order.DAL.UnitOfWork.Interfaces;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IProductApiClient _productService;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork uow, IMapper mapper, IProductApiClient productApiClient, ILogger<CartService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _productService = productApiClient;
            _logger = logger;
        }

        public async Task<OrderResponseModel<CartDTO?>> GetCartAsync(Guid userId, string act = "check")
        {
            var cart = await _uow.Carts.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new OrderResponseModel<CartDTO?>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart not found"
                };
            }

            var dto = _mapper.Map<CartDTO>(cart);

            if (act == "check")
            {
                // Lấy danh sách productId trong cart
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

                // Gọi batch API sang ProductService
                var productResponse = await _productService.GetProductInfoAsync(productIds);

                if (!productResponse.Success || productResponse.Data == null)
                {
                    // Nếu API fail → gắn error message cho toàn bộ items
                    foreach (var item in dto.Items)
                    {
                        item.ErrorMessage = "Cannot fetch product info";
                        item.IsAvailable = false;
                    }

                    return new OrderResponseModel<CartDTO?>
                    {
                        Success = false,
                        Message = OperationResult.Error,
                        ErrorMessage = "Cannot fetch product info",
                        Data = dto
                    };
                }

                // Map product info vào cart items
                var productDict = productResponse.Data.ToDictionary(p => p.ProductId, p => p);

                foreach (var item in dto.Items)
                {
                    if (productDict.TryGetValue(item.ProductId, out var product) && product.IsActive)
                    {
                        item.ProductName = product.ProductName;
                        item.Price = product.SalePrice;
                        item.ErrorMessage = string.Empty;
                        item.IsAvailable = true;
                    }
                    else
                    {
                        item.ErrorMessage = "Product out of stock or unavailable";
                        item.IsAvailable = false;
                    }
                }
            } 

            return new OrderResponseModel<CartDTO?>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }

        public async Task<OrderResponseModel<int>> AddItemToCartAsync(Guid userId, RequestItemToCartModel itemDto)
        {
            // Lấy cart từ DB
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart == null)
            {
                return new OrderResponseModel<int>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart not found"
                };
            }

            // Tìm item đã tồn tại (theo ProductId + StoreId)
            var existingItem = dbCart.Items.FirstOrDefault(i =>
                i.ProductId == itemDto.ProductId && i.StoreId == itemDto.StoreId);

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _uow.Carts.UpdateCartItemAsync(existingItem);
            }
            else
            {
                var newItem = new CartItemModel
                {
                    CartItemId = Guid.NewGuid(),
                    CartId = dbCart.CartId,
                    ProductId = itemDto.ProductId,
                    StoreId = itemDto.StoreId,
                    Quantity = itemDto.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Price = 0,
                    ProductName = string.Empty,
                };

                dbCart.Items.Add(newItem);
                await _uow.Carts.AddCartItemAsync(newItem);
            }

            await _uow.Carts.UpdateCartAsync(dbCart);
            await _uow.SaveChangesAsync();

            return new OrderResponseModel<int>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dbCart.Items.Count
            };
        }



        public async Task<OrderResponseModel<string>> RemoveItemFromCartAsync(Guid userId, Guid productId)
        {
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart == null)
            {
                return new OrderResponseModel<string>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart not found"
                };
            }

            var item = dbCart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                return new OrderResponseModel<string>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Item not found"
                };
            }

            await _uow.Carts.RemoveCartItemAsync(item.CartItemId);
            await _uow.SaveChangesAsync();

            return new OrderResponseModel<string>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = productId.ToString()
            };
        }

        public async Task<OrderResponseModel<string>> ClearCartAsync(Guid userId)
        {
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart == null)
            {
                return new OrderResponseModel<string>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart not found"
                };
            }

            await _uow.Carts.ClearCartAsync(dbCart.CartId);
            await _uow.SaveChangesAsync();

            return new OrderResponseModel<string>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = userId.ToString()
            };
        }

        // Validate & enrich product info
        private async Task<(bool IsValid, string ErrorMessage, List<CartItemModel> ValidItems)>
    ValidateCartItemsAsync(List<CartItemModel> items)
        {
            var validItems = new List<CartItemModel>();
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            // Gọi batch API
            var productResponse = await _productService.GetProductInfoAsync(productIds);

            if (!productResponse.Success || productResponse.Data == null)
            {
                return (false, "Cannot fetch product info", validItems);
            }

            var productDict = productResponse.Data.ToDictionary(p => p.ProductId, p => p);

            foreach (var item in items)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                {
                    return (false, $"Product {item.ProductId} not found or inactive", validItems);
                }

                if (item.Quantity > product.Quantity)
                {
                    return (false, $"Not enough stock for {product.ProductName}. " +
                                   $"Available: {product.Quantity}, Requested: {item.Quantity}", validItems);
                }

                // Update cartItem info nếu có thay đổi
                if (item.Price != product.SalePrice || item.ProductName != product.ProductName)
                {
                    item.Price = product.SalePrice;
                    item.ProductName = product.ProductName;
                    item.UpdatedAt = DateTime.UtcNow;
                    await _uow.Carts.UpdateCartItemAsync(item);
                }

                validItems.Add(item);
            }

            return (true, string.Empty, validItems);
        }


        // Checkout
        public async Task<OrderResponseModel<List<CartItemDTO>>> CheckoutAsync(Guid userId, IEnumerable<Guid> productIds)
        {
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart == null || !dbCart.Items.Any())
            {
                return new OrderResponseModel<List<CartItemDTO>>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart is empty",
                    Data = new List<CartItemDTO>()
                };
            }

            var productIdsList = productIds.ToList();
            var checkoutItems = dbCart.Items.Where(i => productIdsList.Contains(i.ProductId)).ToList();

            if (!checkoutItems.Any())
            {
                return new OrderResponseModel<List<CartItemDTO>>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "No items found for checkout",
                    Data = new List<CartItemDTO>()
                };
            }

            // ✅ VALIDATE các items trước khi checkout
            var (isValid, errorMessage, validItems) = await ValidateCartItemsAsync(checkoutItems);
            if (!isValid)
            {
                return new OrderResponseModel<List<CartItemDTO>>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = errorMessage,
                    Data = new List<CartItemDTO>()
                };
            }

            // Lưu thay đổi nếu có cập nhật thông tin sản phẩm
            await _uow.SaveChangesAsync();

            // Xóa các items đã checkout
            foreach (var item in checkoutItems)
            {
                await _uow.Carts.RemoveCartItemAsync(item.CartItemId);
            }

            // Kiểm tra nếu không còn items nào, xóa cart
            var remainingItems = dbCart.Items.Where(i => !productIdsList.Contains(i.ProductId)).ToList();
            if (!remainingItems.Any())
            {
                await _uow.Carts.DeleteCartAsync(dbCart.CartId);
            }
            else
            {
                await _uow.Carts.UpdateCartAsync(dbCart);
            }

            await _uow.SaveChangesAsync();

            var checkoutItemDtos = _mapper.Map<List<CartItemDTO>>(validItems);
            return new OrderResponseModel<List<CartItemDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = checkoutItemDtos
            };
        }

        public async Task<OrderResponseModel<List<CartItemDTO>>> GetCartItemsByStoreAsync(Guid userId, Guid storeId)
        {
            var items = await _uow.Carts.GetCartItemsByStoreAsync(userId, storeId);
            var itemDtos = _mapper.Map<List<CartItemDTO>>(items);

            return new OrderResponseModel<List<CartItemDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = itemDtos
            };
        }

        public async Task<OrderResponseModel<CountItemsDTO>> CountItemsInCartAsync(Guid userId)
        {
            var cart = await _uow.Carts.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                var newCart = new CartModel
                {
                    UserId = userId,
                    CartId = Guid.NewGuid(),
                    Items = new List<CartItemModel>(),
                    UpdatedAt = DateTime.UtcNow,
                };

                await _uow.Carts.CreateCartAsync(newCart);
                await _uow.SaveChangesAsync();

                var dto = new CountItemsDTO
                {
                    UserId = userId,
                    CartId = newCart.CartId,
                    CountItems = 0
                };

                return new OrderResponseModel<CountItemsDTO>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dto
                };
            }   
            
            var count = cart?.Items.Count ?? 0;

            return new OrderResponseModel<CountItemsDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = new CountItemsDTO
                {
                    UserId = userId,
                    CartId = cart.CartId,
                    CountItems = count
                }
            };

        }

        public async Task<OrderResponseModel<CartDTO>> UpdateItemAsync(Guid buyerId, Guid cartItemId, UpdateQuantityRequest request)
        {
            var cartItem = await _uow.Carts.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null)
            {
                return new OrderResponseModel<CartDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart item not found"
                };
            }

            // Validate product từ ProductService
            var productResponse = await _productService.ValidateProduct(cartItem.ProductId);

            if (!productResponse.Success || productResponse.Data == null)
            {
                // ❌ Product không tồn tại
                cartItem.IsAvailable = false;
                cartItem.ErrorMessage = "Product not found";
            }
            else
            {
                var product = productResponse.Data;

                if (!product.IsActive)
                {
                    // ❌ Product bị inactive
                    cartItem.IsAvailable = false;
                    cartItem.ErrorMessage = "This product is inactive";
                }
                else if (request.Quantity > product.Quantity)
                {
                    // ❌ Không đủ tồn kho
                    cartItem.IsAvailable = false;
                    cartItem.ErrorMessage = $"Insufficient stock, only {product.Quantity} items left";
                }
                else
                {
                    // ✅ Update thành công
                    cartItem.Quantity = request.Quantity;
                    cartItem.Price = product.SalePrice;
                    cartItem.ProductName = product.ProductName;
                    cartItem.IsAvailable = true;
                    cartItem.ErrorMessage = string.Empty;
                }
            }

            cartItem.UpdatedAt = DateTime.UtcNow;
            await _uow.Carts.UpdateCartItemAsync(cartItem);
            await _uow.SaveChangesAsync();

            // ✅ Luôn trả về giỏ hàng mới nhất
            var newCart = await GetCartAsync(buyerId);
            //var dto = _mapper.Map<CartDTO>(newCart);

            return new OrderResponseModel<CartDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = newCart.Data
            };
        }


    }
}