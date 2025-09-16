using System.ComponentModel.DataAnnotations;

namespace Store.Common.Models.Requests
{
    public class StoreActiveModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
