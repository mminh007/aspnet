using Frontend.Enums;
using Frontend.Models.Payments.Requests;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using static Frontend.Models.Payments.DTOs;

namespace Frontend.Controllers
{
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;


        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDTO request, string order_id)
        {

            if (request.OrderId == null)
            {
                request.OrderId = order_id;
                
            }
            
            if(request == null)
            {
                return BadRequest(new { message = "Request are required." });
            }

            var paymentRequest = new PaymentRequest
            {
                OrderId = Guid.Parse(order_id),
                Method = Enum.Parse<PaymentMethod>(request.Method, true),
                Amount = request.Amount,
                Currency = request.Currency ?? "VND"
            };

            var response = await _paymentService.CreatePaymentAsync(paymentRequest);

            return StatusCode(response.StatusCode, new
            {
                message = response.Message,
                data = response.data
            });
    
        }

        [HttpPost("confirm-payment")]         
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {

            if (request == null)
            {
                return BadRequest(new { message = "Request are required." });
            }

            var response = await _paymentService.ConfirmPaymentAsync(request);

            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, new
                {
                    message = response.Message,
                    data = response.data
                });
            }

            return Ok(new
            {
                message = response.Message,
                data = response.data,
            });
        }
    }
}
