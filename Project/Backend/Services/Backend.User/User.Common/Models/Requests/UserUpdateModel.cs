using System.ComponentModel.DataAnnotations;

namespace User.Common.Models.Requests
{
    public class UserUpdateModel
    {
        [Required]
        public Guid UserId { get; set; }

        public Guid? StoreId { get; set; }

        public string Address { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

    }
}
