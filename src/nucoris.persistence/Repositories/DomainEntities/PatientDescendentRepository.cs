using nucoris.domain;
using nucoris.application.interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace nucoris.persistence
{
    public abstract class PatientDescendentRepository<T> :
        IPatientDescendentRepository<T> where T : class, IPatientPersistable
    {
        protected readonly DbSession _dbSession;

        public PatientDescendentRepository(DbSession dbSession)
        {
            _dbSession = dbSession ?? throw new ArgumentNullException(nameof(dbSession));
        }

        public virtual async Task<T> GetAsync(System.Guid patientId, System.Guid itemId)
        {
            return await _dbSession.LoadAsync<T>(patientId, itemId);
        }

        public void Store(T item)
        {
            _dbSession.Register(item);
        }

        public void StoreMany(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Store(item);
            }
        }
    }
}
