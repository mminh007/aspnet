using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Shared.DTO.Products
{
    public class ProductDTOModel
    {
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }

        public int Quantity { get; set; }

        public string Supplier { get; set; }

        public CategoryDTO Category { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
