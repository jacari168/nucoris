using System;

namespace nucoris.persistence
{
    public class MedicationFormRepository : ReferenceDataRepository<
                        application.interfaces.ReferenceData.MedicationForm, Guid>, 
        application.interfaces.repositories.IMedicationFormRepository
    {
        public MedicationFormRepository(DbSession dbSession) : base(dbSession)
        {
        }
    }
}
