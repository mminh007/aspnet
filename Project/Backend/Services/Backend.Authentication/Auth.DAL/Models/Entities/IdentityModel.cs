using System.ComponentModel.DataAnnotations;

namespace Auth.DAL.Models.Entities
{
    public class IdentityModel
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }


        [EmailAddress]
        public string Email { get; set; }

        [Required]  
        public string PasswordHashing { get; set; }

        [Required]
        public string Role { get; set; }

        public string? VerificationCode { get; set; }
        public DateTime? VerificationExpiry { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
    }
}
