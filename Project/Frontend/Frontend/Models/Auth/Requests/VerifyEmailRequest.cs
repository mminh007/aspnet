using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Auth.Requests
{
    public class VerifyEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
