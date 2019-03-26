using System;

namespace nucoris.application.commands
{
    public class EndAdmissionCommand : MediatR.IRequest<CommandResult>
    {
        public Guid PatientId { get; }
        public Guid AdmissionId { get; }
        public DateTime EndTime { get; }

        public EndAdmissionCommand(Guid patientId, Guid admissionId, DateTime endTime)
        {
            PatientId = patientId;
            AdmissionId = admissionId;
            EndTime = endTime;
        }
    }
}
