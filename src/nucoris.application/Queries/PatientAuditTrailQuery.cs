
namespace nucoris.application.queries
{
    public class PatientAuditTrailQuery : MediatR.IRequest<nucoris.queries.PatientAuditTrail.Patient>
    {
        public PatientAuditTrailQuery(System.Guid patientId)
        {
            PatientId = patientId;
        }

        public System.Guid PatientId { get; }
    }
}
