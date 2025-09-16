using AutoMapper;
using Oder.BLL.Services.Interfaces;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using Order.DAL.Models.Entities;
using Order.DAL.UnitOfWork.Interfaces;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;

        public OrderService(IUnitOfWork uow, IMapper mapper, ICartService cartService)
        {
            _uow = uow;
            _mapper = mapper;
            _cartService = cartService;
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

            return new OrderResponseModel<OrderDTO?>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = _mapper.Map<OrderDTO>(entity)
            };
        }

        public async Task<OrderResponseModel<IEnumerable<OrderDTO>>> GetOrdersByUserAsync(Guid userId)
        {
            var entities = await _uow.Orders.GetOrdersByUserAsync(userId);
            return new OrderResponseModel<IEnumerable<OrderDTO>>
            {
                Success = entities != null,
                Message = entities != null ? OperationResult.Success : OperationResult.NotFound,
                Data = entities?.Select(e => _mapper.Map<OrderDTO>(e))
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
        public async Task<OrderResponseModel<IEnumerable<OrderDTO>>> CheckoutAsync(Guid userId, IEnumerable<Guid> productIds)
        {
            // Gọi CartService để checkout và lấy danh sách items
            var checkoutResponse = await _cartService.CheckoutAsync(userId, productIds);

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
                // Tạo Order cho mỗi store
                var orderEntity = new OrderModel
                {
                    OrderId = Guid.NewGuid(),
                    UserId = userId,
                    StoreId = storeGroup.Key,
                    Status = "Pending",
                    TotalAmount = storeGroup.Sum(i => i.Price * i.Quantity),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = storeGroup.Select(item => new OrderItemModel
                    {
                        OrderItemId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                try
                {
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
    }
}
