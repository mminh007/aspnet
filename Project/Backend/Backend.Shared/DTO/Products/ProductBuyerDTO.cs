using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Shared.DTO.Products
{
    public class ProductBuyerDTO
    {
        [MaxLength(500)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        public int Quantity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}