using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.DAL.Models.Entities
{
    public class CartModel
    {
        [Key]
        public Guid CartId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed property - không lưu vào DB
        [NotMapped]
        public decimal TotalPrice => Items?.Sum(item => item.LineTotal) ?? 0;

        [NotMapped]
        public int TotalItems => Items?.Sum(item => item.Quantity) ?? 0;

        // Navigation properties
        public virtual ICollection<CartItemModel> Items { get; set; } = new List<CartItemModel>();
    }
}
