namespace Frontend.Models.Stores
{
    public class SearchKeywordModel
    {
        public string Keyword { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }
      
}
