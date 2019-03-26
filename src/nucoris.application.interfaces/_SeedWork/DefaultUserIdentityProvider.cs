using System;

namespace nucoris.application.interfaces
{
    /// <summary>
    /// This class is used when authentication is disabled, and always returns Guest user as the current user.
    /// </summary>
    public class DefaultUserIdentityProvider : IUserIdentityProvider
    {
        public string CurrentUsername => "guest@nucoris.com";
    }
}
