using System.ComponentModel.DataAnnotations;

namespace Store.Common.Models.Requests
{
    public class UpdateStoreModel
    {
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string? StoreName { get; set; }

        [Required]
        public string? StoreCategory { get; set; }

        [Required]
        public string? StoreImage { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Phone]
        [MaxLength(10)]
        public string Phone { get; set; }
    }
}
