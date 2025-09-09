using System.ComponentModel.DataAnnotations;

namespace Commons.Models.Requests
{
    public class RegisterStoreModel
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
