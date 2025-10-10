namespace Frontend.Models.Stores
{
    public class PaginatedStoreResponse
    {
        public IEnumerable<StoreDto> Stores { get; set; } = new List<StoreDto>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
