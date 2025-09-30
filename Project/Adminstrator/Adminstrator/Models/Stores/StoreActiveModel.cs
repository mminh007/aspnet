namespace Adminstrator.Models.Stores
{
    public class ChangeActiveRequest
    {
        public Guid StoreId { get; set; }
        public bool IsActive { get; set; }
    }
}
