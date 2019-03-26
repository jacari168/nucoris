using nucoris.application.interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.persistence
{
    public abstract class ReferenceDataRepository<T,TIdType> : 
        application.interfaces.IReferenceDataRepository<T, TIdType> where T : class, domain.IReferencePersistable
    {
        protected readonly DbSession _dbSession;

        public ReferenceDataRepository(DbSession dbSession)
        {
            _dbSession = dbSession;
        }

        public async Task<T> GetAsync(TIdType id)
        {
            return await _dbSession.LoadAsync<T>(id.ToString());
        }

        public async Task<List<T>> GetManyAsync(IEnumerable<DbDocumentCondition<T>> whereConditions)
        {
            return await _dbSession.LoadManyAsync<T>(whereConditions);
        }

    }
}
