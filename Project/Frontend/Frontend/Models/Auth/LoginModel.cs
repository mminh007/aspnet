using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Auth
{
    public class LoginModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ClientType { get; set; } = "frontend";
        }
}
