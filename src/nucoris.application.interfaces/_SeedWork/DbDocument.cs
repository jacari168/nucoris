namespace nucoris.application.interfaces
{
    /// <summary>
    /// This class represents the layout of nucoris objects when persisted as JSON documents in a document database.
    /// It's designed to support persistence of objects with different schemata in the same collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbDocument<T>
    {
        /// <summary>
        /// Mandatory identifier in any Cosmos DB document. Usually a GUID converted to string.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Since we store documents with different schemata into a single Cosmos DB collection,
        /// we have to define a "partitionKey" member in every document
        /// and specifiy the collection's path to its partition key as "/partitionKey"
        /// </summary>
        public string partitionKey { get; set; }

        /// <summary>
        /// To easily filter objects, all nucoris documents have a type document
        /// which may have values such as "Event", "Patient", etc.
        /// </summary>
        public string docType { get; set; }

        /// <summary>
        /// To assist with deserialization and troubleshooting in complex scenarios 
        /// this property contains the object's full type name
        /// </summary>
        public string docSubType { get; set; }

        /// <summary>
        /// The actual object (usually a domain aggregate root)
        /// </summary>
        public T docContents { get; set; }
    }
}
