using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payment.BLL.External.Interface;
using Payment.BLL.Services.Interfaces;
using Payment.Common.Enums;
using Payment.Common.Models.Requests;
using Payment.Common.Models.Responses;
using Payment.DAL.Models.Entities;
using Payment.DAL.UnitOfWork.Interfaces;
using Stripe;
using static Payment.Common.Models.DTOs;

namespace Payment.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly IOrderApiClient _orderApiClient;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config, IOrderApiClient orderApiClient,
                              ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderApiClient = orderApiClient;
            _logger = logger;

            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];  // TODO: lấy từ appsettings
            _paymentIntentService = new PaymentIntentService();
        }

        public async Task<PaymentResponseModel<PaymentDTO>> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount),
                    Currency = request.Currency.ToLower(),
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", request.OrderId.ToString() },
                        { "BuyerId", request.BuyerId.ToString() },
                        { "StoreId", request.StoreId.ToString() }
                    }
                };

                var paymentIntent = await _paymentIntentService.CreateAsync(options);

                var payment = new PaymentModel
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    BuyerId = request.BuyerId,
                    Method = request.Method,
                    Status = PaymentStatus.Pending,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    TransactionId = paymentIntent.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<PaymentDTO>(payment);

                dto.ClientSecret = paymentIntent.ClientSecret;

                return new PaymentResponseModel<PaymentDTO>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseModel<PaymentDTO>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentResponseModel<PaymentDTO>> ConfirmPaymentAsync(ConfirmPaymentRequest request)
        {
            try
            {
                // 👉 Không ConfirmAsync nữa, chỉ Get để check trạng thái
                var intent = await _paymentIntentService.GetAsync(request.PaymentIntentId);

                var payment = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == request.PaymentIntentId);
                var entity = payment.FirstOrDefault();

                if (entity == null)
                {
                    return new PaymentResponseModel<PaymentDTO>
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Payment not found"
                    };
                }

                var currency = intent.Currency;             // "vnd", "usd", ...
                var minor = intent.AmountReceived > 0
                                ? intent.AmountReceived
                                : intent.Amount; // ưu tiên AmountReceived
                decimal amount = IsZeroDecimal(currency)
                    ? Convert.ToDecimal(minor)
                    : Convert.ToDecimal(minor) / 100m;

                // ✅ Nếu Stripe trả về đang xử lý, mark Processing
                if (intent.Status == "processing" || intent.Status == "requires_action")
                {
                    entity.MarkProcessing();
                    _unitOfWork.Payments.Update(entity);
                    await _unitOfWork.SaveChangesAsync();

                    // 👉 Trigger OrderService cập nhật trạng thái "Processing"
                    var updateOrder = await _orderApiClient.UpdateStatusOrder(entity.OrderId, "Processing", amount);

                    _logger.LogInformation("Update Status Order: {status}", updateOrder.Success);

                }

                return new PaymentResponseModel<PaymentDTO>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = _mapper.Map<PaymentDTO>(entity)
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseModel<PaymentDTO>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentResponseModel<PaymentDTO>> GetPaymentByIdAsync(Guid paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                return new PaymentResponseModel<PaymentDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Payment not found"
                };

            var dto = _mapper.Map<PaymentDTO>(payment);

            if (payment.Status == PaymentStatus.Pending && !string.IsNullOrEmpty(payment.TransactionId))
            {
                var intent = await _paymentIntentService.GetAsync(payment.TransactionId);
                dto.ClientSecret = intent.ClientSecret;

            }

            return new PaymentResponseModel<PaymentDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }

        public async Task<PaymentResponseModel<PaymentDTO>> GetPaymentByOrderIdAsync(Guid orderId)
        {
            var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
            if (payment == null)
                return new PaymentResponseModel<PaymentDTO>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Payment not found"
                };

            var dto = _mapper.Map<PaymentDTO>(payment);

            if(payment.Status == PaymentStatus.Pending && !string.IsNullOrEmpty(payment.TransactionId))
            {
                var intent = await _paymentIntentService.GetAsync(payment.TransactionId);
                dto.ClientSecret = intent.ClientSecret;
            }

            return new PaymentResponseModel<PaymentDTO>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dto
            };
        }

        public async Task<PaymentResponseModel<IEnumerable<PaymentDTO>>> GetPaymentsByUserAsync(Guid userId)
        {
            var payments = await _unitOfWork.Payments.GetByBuyerAsync(userId);

            if(payments == null)
            {
                return new PaymentResponseModel<IEnumerable<PaymentDTO>>
                {
                    Success = false,
                    Message = OperationResult.NotFound,

                };
            }

            var dtoList = new List<PaymentDTO>();

            foreach (var payment in payments)
            {
                var dto = _mapper.Map<PaymentDTO>(payment);

                if (payment.Status == PaymentStatus.Pending && !string.IsNullOrEmpty(payment.TransactionId))
                {
                    var intent = await _paymentIntentService.GetAsync(payment.TransactionId);
                    dto.ClientSecret = intent.ClientSecret;
                    
                }
                dtoList.Add(dto);
            }

            return new PaymentResponseModel<IEnumerable<PaymentDTO>>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = dtoList
            };
        }

        public async Task<PaymentResponseModel<bool>> CancelPaymentAsync(Guid paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                return new PaymentResponseModel<bool>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Payment not found"
                };

            try
            {
                if (!string.IsNullOrEmpty(payment.TransactionId))
                {
                    await _paymentIntentService.CancelAsync(payment.TransactionId);
                }

                payment.MarkFailed("Canceled by user/system");
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResponseModel<bool>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseModel<bool>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentResponseModel<bool>> UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? message = null)
        {
            var payment = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == paymentIntentId);
            var entity = payment.FirstOrDefault();

            if (entity == null)
            {
                return new PaymentResponseModel<bool>
                {
                    Success = false,
                    Message = OperationResult.NotFound,
                    ErrorMessage = "Payment not found"
                };
            }

            switch (status)
            {
                case PaymentStatus.Completed:
                    entity.MarkCompleted(paymentIntentId, message);
                    break;
                case PaymentStatus.Failed:
                    entity.MarkFailed(message ?? "Payment failed");
                    break;
                case PaymentStatus.Cancelled:
                    entity.MarkFailed("Canceled");
                    break;
                default:
                    entity.MarkProcessing();
                    break;
            }

            _unitOfWork.Payments.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentResponseModel<bool>
            {
                Success = true,
                Message = OperationResult.Success,
                Data = true
            };
        }

        private static bool IsZeroDecimal(string currency)
        {
            // Stripe zero-decimal list (rút gọn những loại hay gặp)
            // https://stripe.com/docs/currencies#zero-decimal
            var c = currency?.Trim().ToLowerInvariant();
            return c is "vnd" or "jpy" or "krw" or "clp" or "xaf" or "xpf" or "vuv" or "bif" or "djf" or "gnf" or "kmf" or "mga" or "pyg" or "rwf" or "xof";
        }
    }
}
