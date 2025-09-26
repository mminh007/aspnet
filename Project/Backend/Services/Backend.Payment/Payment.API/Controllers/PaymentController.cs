using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.BLL.Services.Interfaces;
using Payment.Common.Enums;
using Payment.Common.Models.Requests;
using Payment.Common.Models.Responses;
using Stripe;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // ✅ Create payment
        [HttpPost("create")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            var result = await _paymentService.CreatePaymentAsync(request);
            return HandleResponse(result);
        }

        // ✅ Confirm payment
        [HttpPost("confirm")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            var result = await _paymentService.ConfirmPaymentAsync(request);
            return HandleResponse(result);
        }

        [HttpPost("confirm-test")]
        [AllowAnonymous] // ⚠️ chỉ bật cho DEV, đừng expose ở production
        public async Task<IActionResult> ConfirmPaymentTest([FromBody] ConfirmTestRequest req)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                var confirmOptions = new PaymentIntentConfirmOptions
                {
                    // Stripe test card: 4242 4242 4242 4242
                    PaymentMethod = "pm_card_visa"
                };

                var intent = await paymentIntentService.ConfirmAsync(req.PaymentIntentId, confirmOptions);

                return Ok(new
                {
                    id = intent.Id,
                    status = intent.Status,
                    succeeded = intent.Status == "succeeded"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("{paymentId:guid}")]
        [Authorize(Roles = "buyer,seller,system")]
        public async Task<IActionResult> GetPaymentById(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentByIdAsync(paymentId);
            return HandleResponse(result);
        }

        // ✅ Get payment by OrderId
        [HttpGet("order/{orderId:guid}")]
        [Authorize(Roles = "buyer,seller,system")]
        public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
        {
            var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            return HandleResponse(result);
        }

        // ✅ Get payments by User
        [HttpGet("user/{userId:guid}")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> GetPaymentsByUser(Guid userId)
        {
            var result = await _paymentService.GetPaymentsByUserAsync(userId);
            return HandleResponse(result);
        }

        // ✅ Cancel payment
        [HttpPut("cancel/{paymentId:guid}")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> CancelPayment(Guid paymentId)
        {
            var result = await _paymentService.CancelPaymentAsync(paymentId);
            return HandleResponse(result);
        }

        

        private IActionResult HandleResponse<T>(PaymentResponseModel<T> response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new { statusCode = 200, message = "Operation successful", data = response.Data }),
                OperationResult.Failed => BadRequest(new { statusCode = 400, message = response.ErrorMessage ?? "Operation failed" }),
                OperationResult.Forbidden => StatusCode(403, new { statusCode = 403, message = response.ErrorMessage ?? "Forbidden - You don't have permission" }),
                OperationResult.NotFound => NotFound(new { statusCode = 404, message = response.ErrorMessage ?? "Not found" }),
                OperationResult.Conflict => Conflict(new { statusCode = 409, message = response.ErrorMessage ?? "Conflict occurred" }),
                OperationResult.Error => StatusCode(500, new { statusCode = 500, message = response.ErrorMessage ?? "Unexpected error!" }),
                _ => StatusCode(500, new { statusCode = 500, message = "Unexpected error!" })
            };
        }
    }
}
