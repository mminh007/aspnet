using System.ComponentModel.DataAnnotations;

namespace Store.Common.Models.Requests
{
    public class ChangeActiveRequest
    {
        [Required]
        public Guid StoreId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
