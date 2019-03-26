using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces
{
    // Note that in this prototype users and other reference data are maintained manually,
    //  there is not UI to create new users, so there is no Store method.

    public interface IReferenceDataRepository<T,TIdType> where T : class
    {
        Task<T> GetAsync(TIdType userId);
        Task<List<T>> GetManyAsync(IEnumerable<DbDocumentCondition<T>> whereConditions);
    }
}