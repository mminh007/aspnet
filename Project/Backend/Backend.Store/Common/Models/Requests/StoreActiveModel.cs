using System.ComponentModel.DataAnnotations;

namespace Commons.Models.Requests
{
    public class StoreActiveModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
