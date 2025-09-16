using DAL.Databases;
using DAL.Models.Entities;
using DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace DAL.Repository
{
    public class IntegrationEventLogRepository : IIntegrationEventLogRepository
    {
        private readonly ProductDbContext _db;

        public IntegrationEventLogRepository(ProductDbContext db)
        {
            _db = db;
        }

        public async Task AddEventAsync(IntegrationEventLog evt)
        {
            await _db.IntegrationEventLogs.AddAsync(evt);
            await _db.SaveChangesAsync();
        }

        public async Task<List<IntegrationEventLog>> GetPendingEventsAsync()
        {
            return await _db.IntegrationEventLogs
                .Where(e => e.State == IntegrationEventLog.EventState.NotPublished)
                .ToListAsync();
        }

        public async Task MarkEventAsPublishedAsync(Guid eventId)
        {
            var evt = await _db.IntegrationEventLogs.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt != null)
            {
                evt.State = IntegrationEventLog.EventState.Published;
                evt.PublishedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkEventAsFailedAsync(Guid eventId)
        {
            var evt = await _db.IntegrationEventLogs.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt != null)
            {
                evt.State = IntegrationEventLog.EventState.Failed;
                await _db.SaveChangesAsync();
            }
        }
    }

}
