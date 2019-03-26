using System;
using System.Collections.Generic;

namespace nucoris.domain
{
    // Aggregate roots are the objects that will be persisted (in our case, as JSON documents to Cosmos DB).
    // For consistency across all persisted objects we force their Id property to be a Guid.
    // Additionally they are responsible for tracking events throwed by aggregate roots, which in our domain
    //  are the only entities allowed to raise events, for simplicity.
    public abstract class AggregateRoot : Entity, IPatientPersistable, IDomainEventTracker
    {

        private List<DomainEvent> _events = new List<DomainEvent>();
        [Newtonsoft.Json.JsonIgnore]
        public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

        public PatientIdentity PatientIdentity { get; }

        public AggregateRoot(Guid id, PatientIdentity patientIdentity) : base(id)
        {
            PatientIdentity = patientIdentity;
        }

        public void AddEvent(DomainEvent newEvent)
        {
            _events.Add(newEvent);
        }

        public void ClearEvents()
        {
            _events.Clear();
        }

    }
}