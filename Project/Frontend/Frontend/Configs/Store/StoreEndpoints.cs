namespace Frontend.Configs.Store
{
    public class StoreEndpoints
    {
        public string GetAllStore { get; set; } = string.Empty;
        public string GetStoreDetail { get; set; } = string.Empty;

        public string GetAllPaginated { get; set; } = string.Empty;
        public string GetStoreByKeyword { get; set; } = string.Empty;
        public string GetStoreByTag { get; set; } = string.Empty;
    }
}
