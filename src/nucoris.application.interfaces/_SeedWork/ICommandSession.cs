using System.Threading.Tasks;

namespace nucoris.application.interfaces
{
    public interface ICommandSession
    {
        Task<bool> CommitAsync();
    }
}
