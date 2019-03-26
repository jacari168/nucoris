using System;
using System.Collections.Generic;
using System.Linq;
using nucoris.domain;

namespace nucoris.queries.PatientAuditTrail
{
    public static class PatientAuditTrailFactory
    {
        public static Patient FromDomain(domain.Patient patient, IEnumerable<domain.DomainEvent> events)
        {
            var patientAuditTrail = new Patient()
            {
                Id = patient.Id,
                FamilyName = patient.FamilyName,
                GivenName = patient.GivenName,
                DisplayName = patient.DisplayName,
                Mrn = patient.PatientIdentity.Mrn,
            };

            patientAuditTrail.Items = events.OrderByDescending(i => i.EventTime).Select(i => CreateEvent(i)).ToList();

            return patientAuditTrail;
        }

        private static AuditTrailItem CreateEvent(DomainEvent domainEvent)
        {
            return new AuditTrailItem()
            {
                Description = domainEvent.Description,
                DoneAt = domainEvent.EventTime,
                DoneBy = domainEvent.TriggeredBy?.DisplayName,
                DoneById = domainEvent.TriggeredBy?.Id
            };
        }

    }
}
