using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class DTOs
    {
        public class CategoryDTO
        {
            public Guid? CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public Guid? StoreId { get; set; }
        }

        public class ProductBuyerDTO
        {
            public Guid ProductId { get; set; }
            public Guid StoreId { get; set; }
            public Guid CategoryId { get; set; }
            
            public string ProductName { get; set; }

            public string ProductImage { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal SalePrice { get; set; }

            public int Quantity { get; set; }

            public bool IsActive { get; set; } = true;
        }

        public class ProductDTO
        {
            public Guid StoreId { get; set; }
            public Guid CategoryId { get; set; }
            public string ProductName { get; set; }

            public string Description { get; set; }

            public string ProductImage { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal SalePrice { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal ImportPrice { get; set; }

            public int Quantity { get; set; }

            public string Supplier { get; set; }

            public CategoryDTO? Category { get; set; }
            public bool IsActive { get; set; } = true;
        }

        public class ProductSellerDTO
        {
            public Guid ProductId { get; set; }
            public Guid StoreId { get; set; }
            public Guid CategoryId { get; set; }

            public string ProductName { get; set; }

            public string ProductImage { get; set; }
            public string? Description { get; set; }

            public decimal SalePrice { get; set; }
            public decimal ImportPrice { get; set; }
            public int Quantity { get; set; }

            public string? Supplier { get; set; }
            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class OrderProductDTO
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public decimal SalePrice { get; set; }
            public int Quantity { get; set; }
            public bool IsActive { get; set; } = true;

        }
    }
}
