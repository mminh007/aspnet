using System.ComponentModel.DataAnnotations;

namespace Backend.User.Models
{
    public class UserUpdateModel
    {
        [Required]
        public Guid UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; }
       
        public string Address { get; set; }
 
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
