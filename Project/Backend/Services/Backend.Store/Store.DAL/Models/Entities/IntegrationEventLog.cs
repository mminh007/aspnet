using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.DAL.Models.Entities
{
    public class IntegrationEventLog
    {
        [Key]
        public Guid EventId { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAt { get; set; }

        [Required]
        public EventState State { get; set; } = EventState.NotPublished;

        public enum EventState
        {
            NotPublished = 0,
            InProgress = 1,
            Published = 2,
            Failed = 3
        }
    }
}
