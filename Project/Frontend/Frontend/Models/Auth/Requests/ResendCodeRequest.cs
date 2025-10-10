using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Auth.Requests
{
    public class ResendCodeRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
