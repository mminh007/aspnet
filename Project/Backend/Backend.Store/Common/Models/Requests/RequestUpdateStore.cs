using System.ComponentModel.DataAnnotations;

namespace Commons.Models.Requests
{
    public class RequestUpdateStore
    {
        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? StoreName { get; set; }
    }
}
