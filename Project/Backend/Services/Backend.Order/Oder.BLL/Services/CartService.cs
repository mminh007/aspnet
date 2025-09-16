using AutoMapper;
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

        public CartService(IUnitOfWork uow, IMapper mapper, IProductApiClient productApiClient)
        {
            _uow = uow;
            _mapper = mapper;
            _productService = productApiClient;
        }

        public async Task<OrderResponseModel<CartDTO?>> GetCartAsync(Guid userId)
        {
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (dbCart != null)
            {
                var dto = _mapper.Map<CartDTO>(dbCart);
                return new OrderResponseModel<CartDTO?>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dto
                };
            }

            return new OrderResponseModel<CartDTO?>
            {
                Success = false,
                Message = OperationResult.NotFound,
                ErrorMessage = "Cart not found"
            };
        }

        public async Task<OrderResponseModel<Guid>> AddItemToCartAsync(Guid userId, RequestItemToCartModel itemDto)
        {
            // Lấy cart hiện tại hoặc tạo mới
            var dbCart = await _uow.Carts.GetCartByUserIdAsync(userId);
            var isNewCart = dbCart == null;

            var cartItem = _mapper.Map<CartItemModel>(itemDto);

            // Kiểm tra sản phẩm có tồn tại và active không
            var productResponse = await _productService.GetProductInfoAsync(itemDto.ProductId);
            if (productResponse.Message != OperationResult.Success || productResponse.Data == null || !productResponse.Data.IsActive)
            {
                return new OrderResponseModel<Guid>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = productResponse.ErrorMessage 
                };
            }

            cartItem.Price = productResponse.Data.SalePrice;
            cartItem.ProductName = productResponse.Data.ProductName;

            if (isNewCart)
            {
                dbCart = new CartModel
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    Items = new List<CartItemModel>(),
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Tìm item đã tồn tại (theo cả ProductId + StoreId)
            var existingItem = dbCart.Items.FirstOrDefault(i =>
                i.ProductId == itemDto.ProductId && i.StoreId == itemDto.StoreId);

            if (existingItem != null)
            {
                // Cập nhật quantity cho item đã tồn tại
                existingItem.Quantity += itemDto.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _uow.Carts.UpdateCartItemAsync(existingItem);
            }
            else
            {
                // Thêm item mới
                var newItem = new CartItemModel
                {
                    CartItemId = Guid.NewGuid(),
                    CartId = dbCart.CartId,
                    ProductId = itemDto.ProductId,
                    StoreId = itemDto.StoreId,
                    ProductName = productResponse.Data.ProductName,
                    Price = productResponse.Data.SalePrice,
                    Quantity = itemDto.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                dbCart.Items.Add(newItem);
                await _uow.Carts.AddCartItemAsync(newItem);
            }

            // Lưu cart
            if (isNewCart)
            {
                await _uow.Carts.CreateCartAsync(dbCart);
            }
            else
            {
                await _uow.Carts.UpdateCartAsync(dbCart);
            }

            await _uow.SaveChangesAsync();

            return new OrderResponseModel<Guid>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dbCart.CartId
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
        private async Task<(bool IsValid, string ErrorMessage, List<CartItemModel> ValidItems)> ValidateCartItemsAsync(List<CartItemModel> items)
        {
            var validItems = new List<CartItemModel>();

            foreach (var item in items)
            {
                var productResponse = await _productService.GetProductInfoAsync(item.ProductId);
                if (productResponse.Message != OperationResult.Success || productResponse.Data == null || !productResponse.Data.IsActive)
                {
                    return (false, $"Product {item.ProductId} not found or inactive", validItems);
                }

                if (item.Quantity > productResponse.Data.Quantity)
                {
                    return (false, $"Not enough stock for {productResponse.Data.ProductName}. Available: {productResponse.Data.Quantity}, Requested: {item.Quantity}", validItems);
                }

                // Cập nhật thông tin sản phẩm nếu có thay đổi
                var hasChanges = false;
                if (item.Price != productResponse.Data.SalePrice ||
                    item.ProductName != productResponse.Data.ProductName)
                {
                    item.Price = productResponse.Data.SalePrice;
                    item.ProductName = productResponse.Data.ProductName;
                    item.UpdatedAt = DateTime.UtcNow;
                    await _uow.Carts.UpdateCartItemAsync(item);
                    hasChanges = true;
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

        
    }
}