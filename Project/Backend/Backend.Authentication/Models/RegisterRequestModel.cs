using System.ComponentModel.DataAnnotations;

namespace Backend.Authentication.Models
{
    public class RegisterRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

    }
}
