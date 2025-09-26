using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.BLL.Services.Interfaces;
using Payment.Common.Models.Requests;
using Stripe;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<WebhookController> _logger;
        private readonly IConfiguration _configuration;

        public WebhookController(IPaymentService paymentService, ILogger<WebhookController> logger, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous] // Stripe gửi mà không có JWT
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var endpointSecret = _configuration["Stripe:WebhookSecret"];
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret
                );

                _logger.LogInformation("📩 Stripe event received: {Type}", stripeEvent.Type);

                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                    {
                        _logger.LogInformation("✅ PaymentIntent succeeded: {Id}", paymentIntent.Id);

                        await _paymentService.ConfirmPaymentAsync(new ConfirmPaymentRequest
                        {
                            PaymentIntentId = paymentIntent.Id
                        });
                    }
                }
                else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
                {
                    if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                    {
                        _logger.LogWarning("❌ Payment failed: {Id}", paymentIntent.Id);

                        // Gọi service mark failed
                        var payment = await _paymentService.GetPaymentByOrderIdAsync(
                            Guid.Parse(paymentIntent.Metadata["OrderId"])
                        );

                        if (payment.Success && payment.Data != null)
                        {
                            // Có PaymentId trong DB thì cập nhật
                            await _paymentService.CancelPaymentAsync(payment.Data.PaymentId);
                        }
                    }
                }
                else if (stripeEvent.Type == EventTypes.PaymentIntentCanceled)
                {
                    if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                    {
                        _logger.LogWarning("⚠️ Payment canceled: {Id}", paymentIntent.Id);

                        var payment = await _paymentService.GetPaymentByOrderIdAsync(
                            Guid.Parse(paymentIntent.Metadata["OrderId"])
                        );

                        if (payment.Success && payment.Data != null)
                        {
                            await _paymentService.CancelPaymentAsync(payment.Data.PaymentId);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("ℹ️ Unhandled event type: {Type}", stripeEvent.Type);
                }

                return Ok(new { received = true });
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook error");
                return BadRequest();
            }
        }
    }
}
