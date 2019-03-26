using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces
{
    public interface IPatientDescendentRepository<T> where T : class, nucoris.domain.IPatientPersistable
    {
        Task<T> GetAsync(System.Guid patientId, System.Guid itemId);
        void Store(T item);
        void StoreMany(IEnumerable<T> items);
    }
}
