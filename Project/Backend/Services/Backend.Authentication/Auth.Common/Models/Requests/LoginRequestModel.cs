using System.ComponentModel.DataAnnotations;

namespace Auth.Common.Models.Requests
{
    public class LoginRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string ClientType { get; set; } // e.g., "admin", "frontend"
    }
}
