using System;


namespace nucoris.application.interfaces
{
    /// <summary>
    /// This interface defines the methods a class able to determine the user submitting the request should implement.
    /// </summary>
    public interface IUserIdentityProvider
    {
        string CurrentUsername { get; }
    }
}
