using System;

namespace nucoris.application.commands
{
    public class AdmitPatientCommand : MediatR.IRequest<CommandResultWithCreatedEntityId>
    {
        public Guid PatientId { get; }
        public DateTime AdmissionTime { get; }

        public AdmitPatientCommand(Guid patientId, DateTime admissionTime)
        {
            PatientId = patientId;
            AdmissionTime = admissionTime;
        }
    }
}
