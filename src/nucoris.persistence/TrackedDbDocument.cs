using nucoris.domain;

namespace nucoris.persistence
{
    internal enum DocumentState
    {
        Loaded = 0,
        Added = 1,
        Modified = 2,
        Deleted = 3
    }

    internal static class DocumentStateExtensions
    {
        public static bool IsDirty(this DocumentState self)
        {
            return self == DocumentState.Added
                || self == DocumentState.Modified
                || self == DocumentState.Deleted
                ;
        }
    }

    /// <summary>
    /// This class is a wrapper over DbDocument instances.
    /// It's used by DbSession unit of work to track documents read from the database
    /// or pending being written.
    /// </summary>
    internal class TrackedDbDocument
    {
        public IPersistable Item { get; }
        public string CollectionId { get; }
        public DocumentState State { get; set; }
        public application.interfaces.DbDocument<IPersistable> Document { get; }

        public TrackedDbDocument(IPersistable item, string collectionId, string partitionKey, string docType,
                                DocumentState state)
        {
            this.Item = item;
            this.CollectionId = collectionId;
            this.State = state;

            this.Document = new application.interfaces.DbDocument<IPersistable>()
            {
                id = item.Id.ToString(),
                partitionKey = partitionKey,
                docType = docType,
                docSubType = item.GetType().FullName,
                docContents = item
            };
        }
    }
}
