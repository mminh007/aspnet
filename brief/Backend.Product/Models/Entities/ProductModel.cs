using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Product.Models.Entities
{
    public class ProductModel
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        public Guid StoreId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [MaxLength(500)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }

        public int Quantity { get; set; }


        public string Supplier {  get; set; }


        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public CategoryModel Category { get; set; }
    }
}
