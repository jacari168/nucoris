using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces.repositories;

namespace nucoris.application.queries
{
    public class PatientDetailsQueryHandler : 
        IRequestHandler<PatientDetailsQuery, nucoris.queries.PatientDetails.Patient>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAdmissionRepository _admissionRepository;
        private readonly IOrderRepository _orderRepository;

        public PatientDetailsQueryHandler(
                IPatientRepository patientRepository,
                IAdmissionRepository admissionRepository,
                IOrderRepository orderRepository)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _admissionRepository = admissionRepository ?? throw new ArgumentNullException(nameof(admissionRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<nucoris.queries.PatientDetails.Patient> Handle(
            PatientDetailsQuery request, CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.Condition(request.PatientId == default(Guid), "PatientId");

            var patient = await _patientRepository.GetAsync(request.PatientId);
            Guard.Against.Null(patient, "Cannot find patient");

            var admissions = await _admissionRepository.GetOfPatientAsync(patient.PatientIdentity.Id, activeOnly: false);
            var activeAdmission = admissions.FirstOrDefault(i => i.IsActive);
            List<domain.Order> orders = null;

            if ( activeAdmission != null)
            {
                orders = await _orderRepository.GetOfAdmissionAsync(patient.PatientIdentity.Id, activeAdmission.Id,
                                                    withTheseStates: null);
            }

            var patientDetails = nucoris.queries.PatientDetails.PatientDetailsFactory.FromDomain(patient, admissions, orders);

            return patientDetails;
        }
    }


}
