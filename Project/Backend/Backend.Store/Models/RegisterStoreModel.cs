using System.ComponentModel.DataAnnotations;

namespace Backend.Store.Models
{
    public class RegisterStoreModel
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
