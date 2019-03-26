using System;

namespace nucoris.application.commands
{
    public class AdministerMedicationCommand : MediatR.IRequest<CommandResult>
    {
        public Guid PatientId { get; }
        public Guid MedicationPrescriptionId { get; }
        public Guid AdministeringUserId { get; }
        public DateTime AdministeredAt { get; }

        public AdministerMedicationCommand(Guid patientId, Guid medicationPrescriptionId,
            Guid administeringUserId, DateTime administeredAt            
            )
        {
            PatientId = patientId;
            MedicationPrescriptionId = medicationPrescriptionId;
            AdministeringUserId = administeringUserId;
            AdministeredAt = administeredAt;
        }
    }
}
