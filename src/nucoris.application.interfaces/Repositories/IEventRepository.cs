using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces.repositories
{
    public interface IEventRepository : IPatientDescendentRepository<DomainEvent>
    {        
        Task<IReadOnlyCollection<DomainEvent>> GetPatientEventsAsync(Guid patientId);
    }
}
