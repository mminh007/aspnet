namespace Backend.Shared.DTO.Products

{
    public class CategoryDTO
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        public Guid StoreId { get; set; }
    }
}