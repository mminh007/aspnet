using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDemo
{
    public class Category
    {
        // Category class representing a product category

        public Category()
        {
            Products = new HashSet<Products>();
        }

        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public required string CategoryName { get; set; }

        // Navigation property
        public virtual ICollection<Products> Products { get; set; }
    }
}
