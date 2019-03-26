using nucoris.domain;
using System.Threading.Tasks;

namespace nucoris.application.interfaces.repositories
{
    public interface IUserRepository : IReferenceDataRepository<User,System.Guid>
    {
        Task<User> GetFromUsernameAsync(string username);
    }
}
