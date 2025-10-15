using System.ComponentModel.DataAnnotations;

namespace Adminstrator.Models.Auths.Requests
{
    public class ResendCodeRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
