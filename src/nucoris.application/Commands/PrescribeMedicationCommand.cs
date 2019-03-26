using System;

namespace nucoris.application.commands
{
    public class PrescribeMedicationCommand : MediatR.IRequest<CommandResultWithCreatedEntityId>
    {
        public Guid PatientId { get; }
        public Guid AdmissionId { get; }
        public string MedicationName { get; }
        public decimal DoseAmount { get; }
        public string DoseUnitName { get; }
        public string FrequencySpecification { get; }
        public DateTime StartAt { get; }
        public DateTime EndAt { get; }
        public Guid PrescribingUserId { get; }

        public PrescribeMedicationCommand(Guid patientId, Guid admissionId,
            string medicationName, decimal doseAmount, string doseUnitName,
            string frequencySpecification, DateTime startAt, DateTime endAt,
            Guid prescribingUserId
            )
        {
            PatientId = patientId;
            AdmissionId = admissionId;
            MedicationName = medicationName;
            DoseAmount = doseAmount;
            DoseUnitName = doseUnitName;
            FrequencySpecification = frequencySpecification;
            StartAt = startAt;
            EndAt = endAt;
            PrescribingUserId = prescribingUserId;
        }
    }
}
