using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Common.Models
{
    public class DTOs
    {
        // DTO cho item trong giỏ hàng
        public class CartItemDTO
        {
            public Guid CartItemId { get; set; }
            public Guid ProductId { get; set; }
            public Guid StoreId { get; set; }

            public string ProductName { get; set; } = string.Empty;

            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            public string ErrorMessage { get; set; } = string.Empty;
            public bool IsAvailable { get; set; } = true;
            public decimal Price { get; set; } // lấy từ ProductService
            public string ProductImage { get; set; }
        }

        // DTO cho giỏ hàng
        public class CartDTO
        {
            public Guid CartId { get; set; }
            public Guid UserId { get; set; }

            public ICollection<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
            public decimal TotalPrice => Items?.Sum(i => i.Price * i.Quantity) ?? 0;
            public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;
        }

        // DTO cho item trong order
        public class OrderItemDTO
        {
            public Guid ProductId { get; set; }
            public string ProductImage { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal LineTotal => Quantity * Price;
        }

        // DTO cho order
        public class OrderDTO
        {
            public Guid OrderId { get; set; }
            public Guid UserId { get; set; }
            public Guid StoreId { get; set; }
            public string Status { get; set; } = "Pending";

            public ICollection<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
            public decimal TotalAmount => OrderItems?.Sum(i => i.LineTotal) ?? 0;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public string StoreName { get; set; }
            public string OrderName { get; set; }
        }

        public class CartProductDTO
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public decimal SalePrice { get; set; }
            public int Quantity { get; set; }
            public bool IsActive { get; set; } = true;

        }

        public class  CountItemsDTO
        {
            public Guid UserId { get; set; }
            public Guid CartId { get; set; }
            public int CountItems { get; set; }
        }

        public class StoreDTO
        {
            public Guid? StoreId { get; set; }
            public string StoreName { get; set; }

            public string StoreCategory { get; set; }

            public string Description { get; set; }
            public string StoreImage { get; set; }

            public string Address { get; set; }

            public string Phone { get; set; }

            public bool IsActive { get; set; }

        }
    }
}
