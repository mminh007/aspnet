using System.ComponentModel.DataAnnotations;

namespace Adminstrator.Models.Auths.Requests
{
    public class ResetPasswordRequestModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string VerifyCode { get; set; }

        [Required, MinLength(7)]
        public string NewPassword { get; set; }
    }
}
