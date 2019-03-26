using nucoris.application.interfaces;
using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.persistence
{
    public class AdmissionRepository : PatientDescendentRepository<Admission>,
        application.interfaces.repositories.IAdmissionRepository
    {
        public AdmissionRepository(DbSession dbSession) : base(dbSession)
        {
        }

        public async Task<List<Admission>> GetOfPatientAsync(Guid patientId, bool activeOnly)
        {
            var conditions = new List<DbDocumentCondition<Admission>>();

            if( activeOnly)
            {
                conditions.Add(new DbDocumentCondition<Admission>((a) => a.docContents.IsActive));
            }

            return await _dbSession.LoadManyDerivedAsync(patientId, conditions);
        }
    }
}
