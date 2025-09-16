using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Entities
{
    public class CategoryModel
    {
        [Key]
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        public Guid StoreId { get; set; }

        public ICollection<ProductModel> Products { get; set; }
    }
}
