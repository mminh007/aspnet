using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDemo
{
    public class ProductDetail
    {
        [Key, ForeignKey("Product")]
        public int ProductId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Manufacturer { get; set; }

        // Navigation property
        public virtual Products? Product { get; set; }
    }
}
