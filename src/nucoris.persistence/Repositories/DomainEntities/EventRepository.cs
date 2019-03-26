using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nucoris.application.interfaces;
using nucoris.domain;

namespace nucoris.persistence
{
    public class EventRepository : PatientDescendentRepository<DomainEvent>, 
        application.interfaces.repositories.IEventRepository
    {
        private string _eventDocType;

        public EventRepository(DbSession dbSession, IDbSessionConfiguration dbSessionConfiguration) : base(dbSession)
        {
            if (dbSessionConfiguration == null) throw new ArgumentNullException(nameof(dbSessionConfiguration));
            _eventDocType = dbSessionConfiguration.GetDBDocType<DomainEvent>();
        }


        public async Task<IReadOnlyCollection<DomainEvent>> GetPatientEventsAsync(Guid patientId)
        {
            return await _dbSession.LoadManyDerivedAsync(patientId,
                new List<DbDocumentCondition<DomainEvent>>()
                {
                    new DbDocumentCondition<DomainEvent>((doc) => doc.docType == _eventDocType)
                });
        }
    }
}
