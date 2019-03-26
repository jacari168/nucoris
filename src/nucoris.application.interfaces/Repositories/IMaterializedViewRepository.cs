using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces
{
    public interface IMaterializedViewRepository<R,S>  where R : MaterializedViewItem
                                            where S : IMaterializedViewSpecification<R>
    {
        Task<R> GetAsync(System.Guid id);
        Task<List<R>> GetManyAsync(S specification);
        void Store(R item);
        void Remove(R item);
    }
}
