using Newtonsoft.Json;
using System;

namespace nucoris.domain
{
    /// <summary>
    /// Base class for all domain events.
    /// We derive it from Entity because our domain events behave like entities:
    ///    they have identity and are persisted.
    /// </summary>
    public abstract class DomainEvent : Entity, IPatientPersistable, MediatR.INotification
    {
        // The user sending the command who ended up triggering an event will be set by the application layer,
        //  since it's of no interest to the domain.
        [JsonProperty]
        public User TriggeredBy { get; set; }
        [JsonProperty]
        public DateTime EventTime { get; private set; }

        public PatientIdentity PatientIdentity { get; }

        public abstract string Description { get; }

        public DomainEvent(PatientIdentity patientIdentity) : base(Guid.NewGuid())
        {
            this.EventTime = DateTime.UtcNow;
            this.PatientIdentity = patientIdentity;
        }
    }
}
