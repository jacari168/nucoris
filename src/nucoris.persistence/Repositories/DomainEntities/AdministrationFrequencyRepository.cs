using System;

namespace nucoris.persistence
{
    public class AdministrationFrequencyRepository : ReferenceDataRepository<
                        application.interfaces.ReferenceData.AdministrationFrequency, Guid>, 
        application.interfaces.repositories.IAdministrationFrequencyRepository
    {
        public AdministrationFrequencyRepository(DbSession dbSession) : base(dbSession)
        {
        }
    }
}
