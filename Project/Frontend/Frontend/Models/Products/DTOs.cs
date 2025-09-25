using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frontend.Models.Products
{
    public class DTOs
    {
        public class CategoryDTO
        {
            public Guid CategoryId { get; set; }
            public string CategoryName { get; set; }

            public Guid StoreId { get; set; }
        }

        public class ProductBuyerDTO
        {

            public Guid ProductId { get; set; }
            public Guid StoreId { get; set; }
            public Guid CategoryId { get; set; }
           
            public string ProductName { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            public string ProductImage { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal SalePrice { get; set; }

            public int Quantity { get; set; }

            public bool IsActive { get; set; } = true;
        }
    }
}
