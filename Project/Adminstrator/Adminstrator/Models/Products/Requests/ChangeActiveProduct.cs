namespace Adminstrator.Models.Products.Requests
{
    public class ChangeActiveProduct
    {
        public Guid ProductId { get; set; }
        public bool IsActive { get; set; }
    }
}
