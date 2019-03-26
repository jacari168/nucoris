using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces
{
    /// <summary>
    /// Unit of work after Martin Fowler:
    /// https://martinfowler.com/eaaCatalog/unitOfWork.html
    /// </summary>
    public interface IUnitOfWork
    {
        IReadOnlyCollection<domain.IPersistable> GetTrackedEntities();
        Task<bool> CommitAsync();
        domain.User CurrentUser { get; }
    }
}
