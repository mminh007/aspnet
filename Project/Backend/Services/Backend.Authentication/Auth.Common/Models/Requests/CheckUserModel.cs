using System.ComponentModel.DataAnnotations;

namespace Auth.Common.Models.Requests
{
    public class CheckUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
