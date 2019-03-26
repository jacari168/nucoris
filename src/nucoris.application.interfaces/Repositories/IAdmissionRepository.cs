using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces.repositories
{
    public interface IAdmissionRepository : IPatientDescendentRepository<Admission>
    {
        Task<List<Admission>> GetOfPatientAsync(Guid patientId, bool activeOnly);
    }
}
