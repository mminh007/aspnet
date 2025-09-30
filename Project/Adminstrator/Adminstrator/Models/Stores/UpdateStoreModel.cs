using System.ComponentModel.DataAnnotations;

namespace Adminstrator.Models.Stores
{
    public class UpdateStoreModel
    {
        public Guid StoreId { get; set; }

        public string StoreName { get; set; }  

        public string Description { get; set; }

        public string StoreCategory { get; set; }

        public string StoreImage { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Phone]
        [MaxLength(10)]
        public string Phone { get; set; }
    }
}
