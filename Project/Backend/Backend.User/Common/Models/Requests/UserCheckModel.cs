using System.ComponentModel.DataAnnotations;

namespace Commons.Models.Requests
{
    public class UserCheckModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
