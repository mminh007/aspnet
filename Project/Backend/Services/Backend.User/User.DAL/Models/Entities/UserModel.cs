using System.ComponentModel.DataAnnotations;

namespace User.DAL.Models.Entities
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }

        public Guid? StoreId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }


    }
}

