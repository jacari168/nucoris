
namespace nucoris.application.queries
{
    public class PatientDetailsQuery : MediatR.IRequest<nucoris.queries.PatientDetails.Patient>
    {
        public PatientDetailsQuery(System.Guid patientId)
        {
            PatientId = patientId;
        }

        public System.Guid PatientId { get; }
    }
}
