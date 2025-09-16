using DAL.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Interfaces
{
    public interface IIntegrationEventLogRepository
    {
        Task AddEventAsync(IntegrationEventLog evt);
        Task<List<IntegrationEventLog>> GetPendingEventsAsync();
        Task MarkEventAsPublishedAsync(Guid eventId);
        Task MarkEventAsFailedAsync(Guid eventId);
    }
}
