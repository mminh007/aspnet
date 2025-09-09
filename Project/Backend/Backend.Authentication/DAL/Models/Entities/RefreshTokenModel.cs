using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Entities
{
    public class RefreshTokenModel
    {
        [Key]
        public Guid RefreshTokenId { get; set; }

        public Guid IdentityId { get; set; }

        public byte[] TokenHash { get; set; }

        public DateTime ExpiryDate { get; set; }      

        public DateTime SessionExpiry { get; set; }  

        public DateTime LastActivity { get; set; }    

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }
    }
}
