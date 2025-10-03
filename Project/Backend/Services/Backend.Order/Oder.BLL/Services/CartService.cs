using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.BLL.External.Interfaces;
using Order.Common.Configs;
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

        public async Task<OrderResponseModel<int>> CreateCart(Guid userId)
        {
            var newCart = new CartModel {
                UserId = userId
            };

            await _uow.Carts.CreateCartAsync(newCart);

            var check = await _uow.SaveChangesAsync();

            if (check == 1)
            {
                return new OrderResponseModel<int>
                {
                    Message = OperationResult.Success,
                    Data = check
                };
            }
            else
            {
                return new OrderResponseModel<int>
                {
                    Message = OperationResult.Failed,
                    Data = check,
                };
            }
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

            if (act == "check")
            {
                var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
                var productResponse = await _productService.GetProductInfoAsync(productIds);

                if (!productResponse.Success || productResponse.Data == null)
                {
                    foreach (var item in cart.Items)
                    {
                        item.ErrorMessage = "Cannot fetch product info";
                        item.IsAvailable = false;
                    }

                    return new OrderResponseModel<CartDTO?>
                    {
                        Success = false,
                        Message = OperationResult.Error,
                        ErrorMessage = "Cannot fetch product info",
                        Data = _mapper.Map<CartDTO>(cart)
                    };
                }

                var productDict = productResponse.Data.ToDictionary(p => p.ProductId, p => p);

                foreach (var item in cart.Items) // dùng CartItemModel
                {
                    if (productDict.TryGetValue(item.ProductId, out var product) && product.IsActive)
                    {
                        item.ProductName = product.ProductName;
                        item.ProductImage = product.ProductImage;


                        if (product.Quantity <= 0)
                        {
                            item.Quantity = 0;
                            item.ErrorMessage = "Product out of stock";
                            item.IsAvailable = false;
                        }
                        else if (product.SalePrice != item.Price)
                        {
                            item.Price = product.SalePrice;
                            item.ErrorMessage = "Product has been updated with a new price";
                            item.IsAvailable = true;
                        }    
                        else if (item.Quantity > product.Quantity)
                        {
                            item.Quantity = product.Quantity; // cập nhật entity
                            item.ErrorMessage = $"Maximum quantity available: {product.Quantity}";
                            item.IsAvailable = true;
                        }
                        else
                        {
                            item.ErrorMessage = string.Empty;
                            item.IsAvailable = true;
                        }
                    }
                    else
                    {
                        item.ErrorMessage = "Product not available";
                        item.IsAvailable = false;
                    }

                    item.UpdatedAt = DateTime.UtcNow;
                    await _uow.Carts.UpdateCartItemAsync(item);
                }

                // ❗ Commit thay đổi xuống DB
                await _uow.SaveChangesAsync();
            }

            var dto = _mapper.Map<CartDTO>(cart);

            return new OrderResponseModel<CartDTO?>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }



        public async Task<OrderResponseModel<CartDTO>> AddItemToCartAsync(Guid userId, RequestItemToCartModel itemDto)
        {
            // Lấy cart từ DB
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart == null)
            {
                return new OrderResponseModel<CartDTO>
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
                    ProductImage = string.Empty,
                };

                dbCart.Items.Add(newItem);
                await _uow.Carts.AddCartItemAsync(newItem);
            }

            await _uow.Carts.UpdateCartAsync(dbCart);
            await _uow.SaveChangesAsync();

            var cart = await GetCartAsync(userId, "check");
       
            return new OrderResponseModel<CartDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = cart.Data,
            };
        }



        public async Task<OrderResponseModel<CartDTO>> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart == null)
            {
                return new OrderResponseModel<CartDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart not found"
                };
            }

            var item = dbCart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item == null)
            {
                return new OrderResponseModel<CartDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Item not found"
                };
            }

            await _uow.Carts.RemoveCartItemAsync(item.CartItemId);
            await _uow.SaveChangesAsync();

            var newCart = await GetCartAsync(userId, "");
            return new OrderResponseModel<CartDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = newCart.Data,
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
        private async Task<(bool IsValid, string ErrorMessage, List<CartItemModel> ValidItems)> ValidateCartItemsAsync(List<CartItemModel> items)
        {
            var validItems = new List<CartItemModel>();
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            // batch API
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
                    item.ProductImage = product.ProductImage;
                    item.IsAvailable = false;
                    item.ErrorMessage = $"Product not found or inactive";
                    //return (false, $"Product {item.ProductId} not found or inactive", validItems);
                }

                if (item.Quantity > product.Quantity)
                {
                    item.ProductImage = product.ProductImage;
                    item.ErrorMessage = $"Not enough stock for {product.ProductName}" +
                                        $"Available: {product.Quantity}";
                }

                // Update cartItem info nếu có thay đổi
                if (item.Price != product.SalePrice || item.ProductName != product.ProductName)
                {
                    item.Price = product.SalePrice;
                    item.ProductName = product.ProductName;
                    item.ProductImage = product.ProductImage;
                    item.UpdatedAt = DateTime.UtcNow;
                    item.ErrorMessage = $"Product has just been updated with a new price";
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
            //var (isValid, errorMessage, validItems) = await ValidateCartItemsAsync(checkoutItems);
            //if (!isValid)
            //{
            //    return new OrderResponseModel<List<CartItemDTO>>
            //    {
            //        Success = false,
            //        Message = OperationResult.Error,
            //        ErrorMessage = errorMessage,
            //        Data = new List<CartItemDTO>()
            //    };
            //}

            //// Lưu thay đổi nếu có cập nhật thông tin sản phẩm
            //await _uow.SaveChangesAsync();

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

            var checkoutItemDtos = _mapper.Map<List<CartItemDTO>>(checkoutItems);
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

        public async Task<OrderResponseModel<CartDTO>> UpdateItemAsync(Guid buyerId, Guid productId, UpdateQuantityRequest request)
        {
            var cartItem = await _uow.Carts.GetCartItemByIdAsync(productId);
            if (cartItem == null)
            {
                return new OrderResponseModel<CartDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Cart item not found"
                };
            }

            cartItem.Quantity = request.Quantity;

            cartItem.UpdatedAt = DateTime.UtcNow;
            await _uow.Carts.UpdateCartItemAsync(cartItem);
            await _uow.SaveChangesAsync();

            // ✅ Luôn trả về giỏ hàng mới nhất
            var newCart = await GetCartAsync(buyerId, "check");
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