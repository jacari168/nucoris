using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class AdministerMedicationCommandHandler
        : IRequestHandler<AdministerMedicationCommand, CommandResult>
    {
        private readonly ICommandSession _commandSession;
        private readonly IMedicationPrescriptionRepository _prescriptionRepository;
        private readonly IUserRepository _userRepository;

        public AdministerMedicationCommandHandler(
            ICommandSession commandSession,
            IMedicationPrescriptionRepository medicationPrescriptionRepository,
            IUserRepository userRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _prescriptionRepository = medicationPrescriptionRepository ?? throw new ArgumentNullException(nameof(medicationPrescriptionRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<CommandResult> Handle(AdministerMedicationCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");
            Guard.Against.Condition(cmd.MedicationPrescriptionId == default(Guid), "Invalid prescription id");
            Guard.Against.Condition(cmd.AdministeredAt == default(DateTime), "Invalid administration time");
            Guard.Against.Condition(cmd.AdministeringUserId == default(Guid), "Invalid administering user id");

            var prescription = await _prescriptionRepository.GetAsync(cmd.PatientId, cmd.MedicationPrescriptionId);
            if( prescription == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound,
                    $"Prescription {cmd.MedicationPrescriptionId} for patient {cmd.PatientId} not found.");
            }

            domain.User user = await _userRepository.GetAsync(cmd.AdministeringUserId);
            if (user == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound,
                    $"User {cmd.AdministeringUserId} not found.");
            }

            prescription.Administer(user, cmd.AdministeredAt);
            _prescriptionRepository.Store(prescription);

            var success = await _commandSession.CommitAsync();

            return new CommandResult(success);
        }
    }


}
