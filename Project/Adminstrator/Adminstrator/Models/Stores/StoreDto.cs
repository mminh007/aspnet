using System.Text.Json.Serialization;

namespace Adminstrator.Models.Stores
{
    public class StoreDto
    {
        [JsonPropertyName("StoreId")]
        public Guid StoreId { get; set; }

        [JsonPropertyName("storeName")]
        public string StoreName { get; set; }

        public string StoreImage { get; set; }

        public string StoreCategory { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}
