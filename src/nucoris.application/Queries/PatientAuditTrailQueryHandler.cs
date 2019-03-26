using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.queries
{
    public class PatientAuditTrailQueryHandler : 
        IRequestHandler<PatientAuditTrailQuery, nucoris.queries.PatientAuditTrail.Patient>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IEventRepository _eventRepository;

        public PatientAuditTrailQueryHandler(
                IPatientRepository patientRepository,
                IEventRepository eventRepository)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<nucoris.queries.PatientAuditTrail.Patient> Handle(
            PatientAuditTrailQuery request, CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.Condition(request.PatientId == default(Guid), "PatientId");

            var patient = await _patientRepository.GetAsync(request.PatientId);
            Guard.Against.Null(patient, "Cannot find patient");

            var events = await _eventRepository.GetPatientEventsAsync(request.PatientId);

            var patientAuditTrail = nucoris.queries.PatientAuditTrail.PatientAuditTrailFactory.
                FromDomain(patient, events);

            return patientAuditTrail;
        }
    }


}
