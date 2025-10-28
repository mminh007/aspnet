using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.DAL.Models.Entities
{
    public class ShippingModel
    {
        [Key]
        public Guid ShippingId { get; set; }

        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
