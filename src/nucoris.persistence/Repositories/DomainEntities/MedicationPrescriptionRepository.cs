using nucoris.domain;

namespace nucoris.persistence
{
    public class MedicationPrescriptionRepository : PatientDescendentRepository<MedicationPrescription>, 
        application.interfaces.repositories.IMedicationPrescriptionRepository
    {
        public MedicationPrescriptionRepository(DbSession dbSession) : base(dbSession)
        {
        }
    }
}
