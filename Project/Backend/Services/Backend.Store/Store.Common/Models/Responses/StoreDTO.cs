namespace Store.Common.Models.Responses
{
    public class StoreDTO
    {
        public Guid? StoreId { get; set; } 
        public string StoreName { get; set; }
        
        public string StoreCategory { get; set; }

        public string Description { get; set; }
        public string StoreImage { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }
    }
}
