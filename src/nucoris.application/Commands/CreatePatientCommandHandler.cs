using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class CreatePatientCommandHandler
        : IRequestHandler<CreatePatientCommand, CommandResultWithCreatedEntityId>
    {
        private readonly ICommandSession _commandSession;
        private readonly IPatientRepository _patientRepository;

        public CreatePatientCommandHandler(
            ICommandSession commandSession,
            IPatientRepository patientRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
        }

        public async Task<CommandResultWithCreatedEntityId> Handle(CreatePatientCommand cmd, CancellationToken cancellationToken)
        {
            var existingPatient = await _patientRepository.GetByMrnAsync(cmd.Mrn);

            if( existingPatient != null)
            {
                // Note: in the final application a more elaborate check should be performed,
                //  checking for example whether name and DOB passed in match the existing ones.
                return new CommandResultWithCreatedEntityId(existingPatient.PatientIdentity.Id, CommandResultCode.BadRequest, "Patient already exists.");
            }

            var patient = domain.Patient.NewPatient(cmd.Mrn, cmd.GivenName, cmd.FamilyName, cmd.DateOfBirth);
            _patientRepository.Store(patient);

            var success = await _commandSession.CommitAsync();

            return new CommandResultWithCreatedEntityId(patient.PatientIdentity.Id, success);
        }
    }


}
