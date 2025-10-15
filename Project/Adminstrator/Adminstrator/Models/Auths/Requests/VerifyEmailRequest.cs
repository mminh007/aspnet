using System.ComponentModel.DataAnnotations;

namespace Adminstrator.Models.Auths.Requests
{
    public class VerifyEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
