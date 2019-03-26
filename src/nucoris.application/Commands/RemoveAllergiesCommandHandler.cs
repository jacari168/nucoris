using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class RemoveAllergiesCommandHandler
        : IRequestHandler<RemoveAllergiesCommand, CommandResult>
    {
        private readonly ICommandSession _commandSession;
        private readonly IPatientRepository _patientRepository;

        public RemoveAllergiesCommandHandler(
            ICommandSession commandSession,
            IPatientRepository patientRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
        }

        public async Task<CommandResult> Handle(RemoveAllergiesCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");
            Guard.Against.Null(cmd.Allergies, "No allergy specified");

            var patient = await _patientRepository.GetAsync(cmd.PatientId);

            if( patient == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound, "Patient not found.");
            }

            foreach (var allergy in cmd.Allergies)
            {
                patient.Remove(new domain.Allergy(allergy));
            }
            _patientRepository.Store(patient);

            var success = await _commandSession.CommitAsync();

            return new CommandResult(success);
        }
    }


}
