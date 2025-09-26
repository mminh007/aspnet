using Payment.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Payment.DAL.Models.Entities
{
    public class PaymentModel
    {
        [Key]
        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid BuyerId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "VND"; // hoặc USD

        // Thông tin cơ bản về giao dịch với cổng thanh toán
        public string? TransactionId { get; set; }    // ID từ cổng thanh toán
        public string? GatewayResponse { get; set; } // lưu JSON raw hoặc message
        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Simple Business logic 
        public void MarkProcessing()
        {
            Status = PaymentStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkCompleted(string? transactionId = null, string? gatewayResponse = null)
        {
            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            GatewayResponse = gatewayResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            FailureReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
