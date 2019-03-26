namespace nucoris.domain
{
    /// <summary>
    /// This interface is used to mark reference data domain objects (dictionaries such as users)
    /// that can be persisted, that is, retrieved from a repository.
    /// </summary>
    public interface IReferencePersistable : IPersistable
    {
    }
}