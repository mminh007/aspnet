using EntityFrameworkDemo;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Products
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Column("UnitPrice", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [NotMapped]
    public string? Temp { get; set; }

    // 🔗 Relationship: Many-to-One with Category
    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    // 🔗 Relationship: One-to-One with ProductDetail
    public virtual required ProductDetail ProductDetail { get; set; }
}
