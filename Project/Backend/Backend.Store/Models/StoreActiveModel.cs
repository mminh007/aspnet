using System.ComponentModel.DataAnnotations;

namespace Backend.Store.Models
{
    public class StoreActiveModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
