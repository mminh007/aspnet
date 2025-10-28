using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.BLL.External;
using Payment.BLL.External.Interface;
using Payment.BLL.Services.Interfaces;
using Payment.Common.Enums;
using Payment.Common.Models.Requests;
using Stripe;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderApiClient _orderApiClient;
        private readonly IStoreApiClient _storeApiClient;
        private readonly ILogger<WebhookController> _logger;
        private readonly IConfiguration _configuration;


        public WebhookController(IPaymentService paymentService, ILogger<WebhookController> logger, IConfiguration configuration,
                                 IOrderApiClient orderApiClient)
        {
            _paymentService = paymentService;
            _orderApiClient = orderApiClient;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous] 
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

                        await _paymentService.UpdatePaymentStatusAsync(paymentIntent.Id, PaymentStatus.Completed);

                        var orderId = Guid.Parse(paymentIntent.Metadata["OrderId"]);
                        var storeId = Guid.Parse(paymentIntent.Metadata["StoreId"]);
                        var order = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                        var currency = paymentIntent.Currency;             // "vnd", "usd", ...
                        var minor = paymentIntent.AmountReceived > 0
                                        ? paymentIntent.AmountReceived
                                        : paymentIntent.Amount; // ưu tiên AmountReceived
                        decimal amount = IsZeroDecimal(currency)
                            ? Convert.ToDecimal(minor)
                            : Convert.ToDecimal(minor) / 100m;

                        // 4) Gọi sang Store để cộng số dư (idempotency dùng paymentIntent.Id)
                        var settleReq = new StoreSettleRequest
                        {
                            StoreId = storeId,
                            Amount = amount,
                            IdempotencyKey = paymentIntent.Id,
                            PaymentId = paymentIntent.Id
                        };

                        //_logger.LogInformation("➡️ Calling Store.Settlements StoreId={StoreId} Amount={Amount}", storeId, amount);
                        //var creditRes = await _storeApiClient.CreditAsync(settleReq, HttpContext.RequestAborted);

                        //if (!creditRes.Success)
                        //{
                        //    _logger.LogError("❌ Store credit failed. StoreId={StoreId}, PI={PI}", storeId, paymentIntent.Id);
                        //    // Tùy chiến lược: vẫn tiếp tục cập nhật Order để Stripe không retry,
                        //    // hoặc dừng tại đây (return 500) để Stripe retry webhook.
                        //    // Ở đây: ghi log và tiếp tục.
                        //}
                        //else
                        //{
                        //    _logger.LogInformation("🟩 Store credited successfully. StoreId={StoreId}, Amount={Amount}", storeId, amount);
                        //}

                        await _orderApiClient.UpdateStatusOrder(orderId, "Paid", amount);
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

        private static bool IsZeroDecimal(string currency)
        {
            // Stripe zero-decimal list (rút gọn những loại hay gặp)
            // https://stripe.com/docs/currencies#zero-decimal
            var c = currency?.Trim().ToLowerInvariant();
            return c is "vnd" or "jpy" or "krw" or "clp" or "xaf" or "xpf" or "vuv" or "bif" or "djf" or "gnf" or "kmf" or "mga" or "pyg" or "rwf" or "xof";
        }
    }
}
