using Order.DAL.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderItemModel
{
    [Key]
    public Guid OrderItemId { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; } = 0;

    // Computed property
    [NotMapped]
    public decimal LineTotal => Quantity * Price;

    // Navigation properties
    [ForeignKey("OrderId")]
    public virtual OrderModel? Order { get; set; }
}
