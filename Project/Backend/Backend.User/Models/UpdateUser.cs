using System.ComponentModel.DataAnnotations;

namespace Backend.User.Models
{
    public class UpdateUser
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Address { get; set; }

        [Required] 
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
