using System.ComponentModel.DataAnnotations;

namespace Store.Common.Models.Requests
{
    public class UpdateStoreModel
    {
        public Guid storeId { get; set; }


        [MaxLength(500)]
        public string? Description { get; set; }


        [MaxLength(50)]
        public string? StoreName { get; set; }


        public string? StoreCategory { get; set; }


        public string? StoreImage { get; set; }


        public string Address { get; set; }


        [Phone]
        [MaxLength(10)]
        public string Phone { get; set; }
    }
}
