using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class PrescribeMedicationCommandHandler
        : IRequestHandler<PrescribeMedicationCommand, CommandResultWithCreatedEntityId>
    {
        private readonly ICommandSession _commandSession;
        private readonly IAdmissionRepository _admissionRepository;
        private readonly IMedicationPrescriptionRepository _prescriptionRepository;
        private readonly IUserRepository _userRepository;

        public PrescribeMedicationCommandHandler(
            ICommandSession commandSession,
            IAdmissionRepository admissionRepository,
            IMedicationPrescriptionRepository medicationPrescriptionRepository,
            IUserRepository userRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _admissionRepository = admissionRepository ?? throw new ArgumentNullException(nameof(admissionRepository));
            _prescriptionRepository = medicationPrescriptionRepository ?? throw new ArgumentNullException(nameof(medicationPrescriptionRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<CommandResultWithCreatedEntityId> Handle(PrescribeMedicationCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");
            Guard.Against.Condition(cmd.AdmissionId == default(Guid), "Invalid admission id");
            Guard.Against.Condition(cmd.DoseAmount <= 0, "Invalid dose amount");
            Guard.Against.NullOrWhiteSpace(cmd.DoseUnitName, "Dose unit");
            Guard.Against.Condition(cmd.StartAt == default(DateTime), "Invalid start time");
            Guard.Against.Condition(cmd.EndAt == default(DateTime), "Invalid end time");
            Guard.Against.NullOrWhiteSpace(cmd.FrequencySpecification, "Frequency");
            Guard.Against.NullOrWhiteSpace(cmd.MedicationName, "Medication name");
            Guard.Against.Condition(cmd.PrescribingUserId == default(Guid), "Invalid prescribing user id");

            var admission = await _admissionRepository.GetAsync(cmd.PatientId, cmd.AdmissionId);
            if( admission == null)
            {
                return new CommandResultWithCreatedEntityId(cmd.AdmissionId, CommandResultCode.EntityNotFound,
                    $"Admission {cmd.AdmissionId} for patient {cmd.PatientId} not found.");
            }

            domain.User user = await _userRepository.GetAsync(cmd.PrescribingUserId);
            if (user == null)
            {
                return new CommandResultWithCreatedEntityId(cmd.PrescribingUserId, CommandResultCode.EntityNotFound,
                    $"User {cmd.PrescribingUserId} not found.");
            }

            var prescription = admission.Prescribe(new domain.Medication(cmd.MedicationName),
                new domain.Dose(cmd.DoseAmount, new domain.Unit(cmd.DoseUnitName)),
                new domain.Frequency(cmd.FrequencySpecification), cmd.StartAt, cmd.EndAt,
                user, prescribedAt: DateTime.UtcNow);

            _prescriptionRepository.Store(prescription);

            var success = await _commandSession.CommitAsync();

            return new CommandResultWithCreatedEntityId(prescription.Id, success);
        }
    }


}
