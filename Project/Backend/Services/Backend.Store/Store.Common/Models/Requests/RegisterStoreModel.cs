using System.ComponentModel.DataAnnotations;

namespace Store.Common.Models.Requests
{
    public class RegisterStoreModel
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
