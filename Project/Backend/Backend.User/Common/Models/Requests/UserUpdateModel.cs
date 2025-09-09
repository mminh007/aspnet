using System.ComponentModel.DataAnnotations;

namespace Commons.Models.Requests
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
