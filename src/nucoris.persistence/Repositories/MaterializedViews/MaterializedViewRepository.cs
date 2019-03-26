using nucoris.application.interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.persistence
{
    public class MaterializedViewRepository<R,S> : IMaterializedViewRepository<R,S>
                where R : MaterializedViewItem
                where S : IMaterializedViewSpecification<R>
    {
        private readonly DbSession _dbSession;

        public MaterializedViewRepository(DbSession dbSession)
        {
            _dbSession = dbSession;
        }

        public async Task<R> GetAsync(System.Guid itemId)
        {
            return await _dbSession.LoadAsync<R>(itemId);
        }

        public async Task<List<R>> GetManyAsync(S specification)
        {
            return await _dbSession.LoadManyAsync<R>(specification.AsLinqExpressions());
        }

        public void Remove(R item)
        {
            _dbSession.RegisterForDeletion(item);
        }

        public void Store(R item)
        {
            _dbSession.Register(item);
        }
    }
}
