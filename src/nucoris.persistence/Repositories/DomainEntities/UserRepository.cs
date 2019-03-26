using nucoris.application.interfaces;
using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.persistence
{
    public class UserRepository : ReferenceDataRepository<User,Guid>, 
        application.interfaces.repositories.IUserRepository
    {
        public UserRepository(DbSession dbSession) : base(dbSession)
        {
        }

        public async Task<User> GetFromUsernameAsync(string username)
        {
            // GetUserFromUsernameAsync is implemented by DbSession because it's used there to determine userId from username logged on.
            // We can't place the implementation here it depends on DbSession and would create a circular dependency.
            return await _dbSession.GetUserFromUsernameAsync(username);
        }
    }
}
