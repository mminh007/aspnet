using System.ComponentModel.DataAnnotations;

namespace Commons.Models.Requests
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
