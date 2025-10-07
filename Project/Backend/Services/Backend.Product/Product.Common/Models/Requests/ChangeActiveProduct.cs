using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Common.Models.Requests
{
    public class ChangeActiveProduct
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
