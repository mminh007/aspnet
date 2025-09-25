using System.ComponentModel.DataAnnotations;

namespace Backend.Authentication.Models.Entity
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
    }
}
