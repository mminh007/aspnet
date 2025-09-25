using System.ComponentModel.DataAnnotations;

namespace Backend.User.Models.Requests
{
    public class UserCheckModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
