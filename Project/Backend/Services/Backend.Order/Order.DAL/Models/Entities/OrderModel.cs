

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.DAL.Models.Entities
{
    
    public class OrderModel
    {
        [Key]
        public Guid OrderId { get; set; }

        [Required]
        public Guid UserId { get; set; }    // user = buyer

        [Required]
        public Guid StoreId { get; set; }   // store = seller

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "None"; // e.g., None, Pending, Success 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]        
        public string StoreName { get; set; }
        public string OrderName { get; set; }
        // Navigation properties
        public virtual ICollection<OrderItemModel> OrderItems { get; set; } = new List<OrderItemModel>();
    }
}
