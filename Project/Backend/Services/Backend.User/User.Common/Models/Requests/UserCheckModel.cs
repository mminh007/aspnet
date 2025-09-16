using System.ComponentModel.DataAnnotations;

namespace User.Common.Models.Requests
{
    public class UserCheckModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

    }
}
