using System.ComponentModel.DataAnnotations;

namespace Store.Common.Models.Requests
{
    public class RequestUpdateStore
    {
        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? StoreName { get; set; }
    }
}
