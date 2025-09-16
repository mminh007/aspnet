using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Interfaces
{
    public interface IIntegrationEvent
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
    }

    public abstract class IntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; private set; }
        public DateTime CreationDate { get; private set; }

        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
    }
}
