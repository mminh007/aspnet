using AutoMapper;
using Order.BLL.Services.Interfaces;
using Order.BLL.External.Interfaces;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using Order.DAL.Models.Entities;
using Order.DAL.UnitOfWork.Interfaces;
using static Order.Common.Models.DTOs;
using System.Security.Cryptography;

namespace Order.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;
        private readonly IProductApiClient _productService;
        private readonly IStoreApiClient _storeService;

        public OrderService(IUnitOfWork uow, IMapper mapper, ICartService cartService, IProductApiClient productApiClient,
                            IStoreApiClient storeApiClient)
        {
            _uow = uow;
            _mapper = mapper;
            _cartService = cartService;
            _productService = productApiClient;
            _storeService = storeApiClient;
        }

        public async Task<OrderResponseModel<OrderDTO>> CreateOrderAsync(OrderDTO dto)
        {
            var entity = _mapper.Map<OrderModel>(dto);
            await _uow.Orders.CreateOrderAsync(entity);
            await _uow.SaveChangesAsync();

            return new OrderResponseModel<OrderDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = _mapper.Map<OrderDTO>(entity)
            };
        }

        public async Task<OrderResponseModel<OrderDTO?>> GetOrderByIdAsync(Guid orderId)
        {
            var entity = await _uow.Orders.GetOrderByIdAsync(orderId);

            if (entity == null)
                return new OrderResponseModel<OrderDTO?>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Order not found"
                };

            var productIds = entity.OrderItems.Select(i => i.ProductId).Distinct().ToList();

            var productResponse = await _productService.GetProductInfoAsync(productIds);

            // Map entity -> DTO
            var orderDto = _mapper.Map<OrderDTO>(entity);

            if (productResponse.Success && productResponse.Data != null)
            {
                // Tạo dictionary để lookup nhanh product info theo ProductId
                var productDict = productResponse.Data.ToDictionary(p => p.ProductId, p => p);

                foreach (var item in orderDto.OrderItems)
                {
                    if (productDict.TryGetValue(item.ProductId, out var product))
                    {
                        item.ProductName = product.ProductName;
                        item.ProductImage = product.ProductImage;
                    }
                }
            }
            else
            {
                // Nếu không gọi được ProductService thì vẫn trả order, nhưng có cảnh báo
                return new OrderResponseModel<OrderDTO?>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = orderDto,
                    ErrorMessage = "Cannot call Product Service"
                };
            }

            return new OrderResponseModel<OrderDTO?>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = orderDto
            };
        }

        public async Task<OrderResponseModel<IEnumerable<OrderDTO>>> GetOrdersByUserAsync(Guid userId)
        {
            var entities = await _uow.Orders.GetOrdersByUserAsync(userId);

            if (entities == null || !entities.Any())
            {
                return new OrderResponseModel<IEnumerable<OrderDTO>>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "No orders found for this user"
                };
            }

            // Lấy tất cả productId trong toàn bộ order
            var productIds = entities.SelectMany(o => o.OrderItems)
                                     .Select(i => i.ProductId)
                                     .Distinct()
                                     .ToList();

            // Gọi ProductService để lấy thông tin sản phẩm
            var productResponse = await _productService.GetProductInfoAsync(productIds);

             //var storeinfo = await _storeService.GetStoreInfoAsync(storeGroup.Key);

            // Map entity -> DTO
            var orderDtos = entities.Select(e => _mapper.Map<OrderDTO>(e)).ToList();

            if (productResponse.Success && productResponse.Data != null)
            {
                // Tạo dictionary để lookup nhanh product info theo ProductId
                var productDict = productResponse.Data.ToDictionary(p => p.ProductId, p => p);

                foreach (var order in orderDtos)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (productDict.TryGetValue(item.ProductId, out var product))
                        {
                            item.ProductName = product.ProductName;
                            item.ProductImage = product.ProductImage;
                        }
                    }
                }
            }
            else
            {
                // Nếu không gọi được ProductService thì vẫn trả order, nhưng có cảnh báo
                return new OrderResponseModel<IEnumerable<OrderDTO>>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = orderDtos,
                    ErrorMessage = "Cannot call Product Service"
                };
            }

            return new OrderResponseModel<IEnumerable<OrderDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = orderDtos
            };
        }


        public async Task<OrderResponseModel<IEnumerable<OrderDTO>>> GetOrdersByStoreAsync(Guid storeId)
        {
            var entities = await _uow.Orders.GetOrdersByStoreAsync(storeId);
            return new OrderResponseModel<IEnumerable<OrderDTO>>
            {
                Success = entities != null,
                Message = entities != null ? OperationResult.Success : OperationResult.NotFound,
                Data = entities?.Select(e => _mapper.Map<OrderDTO>(e))
            };
        }

        public async Task<OrderResponseModel<OrderDTO>> UpdateOrderAsync(UpdateOrderRequest dto)
        {
            var existingOrder = await _uow.Orders.GetOrderByIdAsync(dto.OrderId);
            if (existingOrder == null)
            {
                return new OrderResponseModel<OrderDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Order not found"
                };
            }
            // Chỉ cập nhật các trường được phép
            existingOrder = _mapper.Map(dto, existingOrder);

            await _uow.Orders.UpdateOrderAsync(existingOrder);
            await _uow.SaveChangesAsync();

            return new OrderResponseModel<OrderDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = _mapper.Map<OrderDTO>(existingOrder)
            };
        }

        public async Task<OrderResponseModel<string>> DeleteOrderAsync(Guid orderId)
        {
            await _uow.Orders.DeleteOrderAsync(orderId);
            await _uow.SaveChangesAsync();

            return new OrderResponseModel<string>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = "Order deleted successfully"
            };
        }

        // ✅ Checkout
        public async Task<OrderResponseModel<IEnumerable<OrderDTO>>> CheckoutAsync(Guid userId, RequestOrderModel request)
        {
            // Gọi CartService để checkout và lấy danh sách items
            var checkoutResponse = await _cartService.CheckoutAsync(userId, request.ProductIds);

            // Kiểm tra response từ CartService
            if (!checkoutResponse.Success || checkoutResponse.Data == null || !checkoutResponse.Data.Any())
            {
                return new OrderResponseModel<IEnumerable<OrderDTO>>
                {
                    Success = false,
                    Message = checkoutResponse.Message,
                    ErrorMessage = checkoutResponse.ErrorMessage ?? "No valid products selected for checkout"
                };
            }

            var checkoutItems = checkoutResponse.Data;

            // Group items theo StoreId để tạo multiple orders
            var groupedByStore = checkoutItems.GroupBy(i => i.StoreId);
            var createdOrders = new List<OrderModel>();

            foreach (var storeGroup in groupedByStore)
            {
                var storeinfo = await _storeService.GetStoreInfoAsync(storeGroup.Key); 

                var shippInfo = new ShippingModel
                {
                    ShippingId = Guid.NewGuid(),
                    Address = request.Address,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Note = request.Note
                };

                // Tạo Order cho mỗi store
                var orderEntity = new OrderModel
                {
                    OrderId = Guid.NewGuid(),
                    UserId = userId,
                    StoreId = storeGroup.Key,
                    Status = "Pending",
                    TotalAmount = storeGroup.Sum(i => i.Price * i.Quantity),
                    StoreName = storeinfo.Data.StoreName,
                    OrderName = $"SPF{DateTime.UtcNow:yyMMdd}{GenerateRandomDigits(7)}",
                    ShippingId = shippInfo.ShippingId,
                    Shipping = shippInfo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = storeGroup.Select(item => new OrderItemModel
                    {
                        OrderItemId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        ProductImage = item.ProductImage,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                try
                {
                    // Tạo shipping record
                    await _uow.Shippings.AddAsync(shippInfo);
                    await _uow.Orders.CreateOrderAsync(orderEntity);
                    createdOrders.Add(orderEntity);
                }
                catch (Exception ex)
                {
                    // Log error và rollback nếu cần
                    // Có thể implement compensation logic ở đây
                    return new OrderResponseModel<IEnumerable<OrderDTO>>
                    {
                        Success = false,
                        Message = OperationResult.Error,
                        ErrorMessage = $"Failed to create order for store {storeGroup.Key}: {ex.Message}"
                    };
                }
            }

            try
            {
                // Save tất cả orders cùng lúc
                await _uow.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new OrderResponseModel<IEnumerable<OrderDTO>>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = $"Failed to save orders: {ex.Message}"
                };
            }

            // Map sang DTO và return
            var orderDtos = createdOrders.Select(o => _mapper.Map<OrderDTO>(o)).ToList();

            return new OrderResponseModel<IEnumerable<OrderDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = orderDtos
            };
        }

        public async Task<OrderResponseModel<string>> UpdateStatusAsync(Guid orderId, string status, decimal Amount)
        {
            //if (!decimal.TryParse(Amount, out var total))
            //{
            //    return new OrderResponseModel<string>
            //    {
            //        Success = false,
            //        Message = OperationResult.Failed,
            //        ErrorMessage = "Invalid amount format"
            //    };
            //}

            await _uow.Orders.UpdateStatus(orderId, status, Amount);

            await _uow.SaveChangesAsync();

            return new OrderResponseModel<string>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = status
            };
        }

        private static string GenerateRandomDigits(int length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Chuyển byte sang số (0–9)
            var digits = bytes.Select(b => (b % 10).ToString());
            return string.Concat(digits);
        }
    }
}
