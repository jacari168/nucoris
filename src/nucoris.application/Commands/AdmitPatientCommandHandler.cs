using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class AdmitPatientCommandHandler
        : IRequestHandler<AdmitPatientCommand, CommandResultWithCreatedEntityId>
    {
        private readonly ICommandSession _commandSession;
        private readonly IPatientRepository _patientRepository;
        private readonly IAdmissionRepository _admissionRepository;

        public AdmitPatientCommandHandler(
            ICommandSession commandSession,
            IPatientRepository patientRepository,
            IAdmissionRepository admissionRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _admissionRepository = admissionRepository ?? throw new ArgumentNullException(nameof(admissionRepository));
        }

        public async Task<CommandResultWithCreatedEntityId> Handle(AdmitPatientCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");

            var patient = await _patientRepository.GetAsync(cmd.PatientId);

            if( patient == null)
            {
                return new CommandResultWithCreatedEntityId(cmd.PatientId, CommandResultCode.EntityNotFound, "Patient not found.");
            }

            var admission = patient.Admit(cmd.AdmissionTime);
            _admissionRepository.Store(admission);

            var success = await _commandSession.CommitAsync();

            return new CommandResultWithCreatedEntityId(admission.Id, success);
        }
    }


}
