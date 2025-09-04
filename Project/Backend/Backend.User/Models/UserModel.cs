using System.ComponentModel.DataAnnotations;

namespace Backend.User.Models
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}

