using System.ComponentModel.DataAnnotations;

namespace Backend.User.Models.Requests
{
    public class UserUpdateModel
    {
        [Required]
        public Guid UserId { get; set; }
       
        public string Address { get; set; }
 
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
