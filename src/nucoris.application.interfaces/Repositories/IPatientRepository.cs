
using System.Collections.Generic;
using System.Threading.Tasks;
using nucoris.domain;

namespace nucoris.application.interfaces.repositories
{
    public interface IPatientRepository : IPatientDescendentRepository<Patient>
    {
        Task<Patient> GetByMrnAsync(string mrn);
        Task<Patient> GetAsync(System.Guid patientId);
        Task<List<IPatientPersistable>> GetFullPatientRecordAsync(System.Guid patientId);
    }
}
