namespace nucoris.persistence
{
    /// <summary>
    /// This class is just a way to keep together the 3 main properties used to store nucoris objects in the database (Cosmos DB)
    /// </summary>
    public sealed class DbTypeConfiguration
    {
        public DbTypeConfiguration(string collectionId, string partitionKey, string docType)
        {
            CollectionId = collectionId;
            PartitionKey = partitionKey;
            DocType = docType;
        }

        public string CollectionId { get; }
        public string PartitionKey { get; }
        public string DocType { get; }
    }
}
