using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.DAL.Models.Entities
{
    public class StoreModel
    {
        [Key]
        public Guid StoreId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string? StoreName { get; set; }

        public string? StoreCategory { get; set; }

        [MaxLength(255)]
        public string? StoreCategorySlug { get; set; }
        public string? StoreImage { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = false;

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
