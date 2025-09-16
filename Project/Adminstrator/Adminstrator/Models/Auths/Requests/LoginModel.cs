using System.ComponentModel.DataAnnotations;

namespace Adminstrator.Models.Auths.Requests
{
    public class LoginModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ClientType { get; set; } = "admin"; // frontend, admin, ...
    }
}
