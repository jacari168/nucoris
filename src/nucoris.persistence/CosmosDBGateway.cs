using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using nucoris.application.interfaces;

namespace nucoris.persistence
{
    /// <summary>
    /// This class encapsulates the access to Cosmos DB, as you can deduce from its name.
    /// </summary>
    public class CosmosDBGateway : IDisposable
    { 
        private readonly Uri _endpoint;
        private readonly string _authKey;
        private readonly string _databaseId;
        private readonly string _persistenceSpId;
        private DocumentClient _client;
        private readonly object _clientLock = new object();

        public CosmosDBGateway(string endpoint, string authKey)
        {
            _endpoint = new Uri(endpoint);
            _authKey = authKey;

            // In the final application these properties could be read from config,
            //  in this prototype it's not worth the effort:
            _databaseId = "nucorisDb"; 
            _persistenceSpId = "spPersistDocuments";
        }

        internal async Task<bool> CommitAsync(IEnumerable<TrackedDbDocument> documents)
        {
            // Cosmos DB supports transactions, BUT only within stored procedures.
            // Given that SPs in Cosmos DB are registered into a collection and are scoped by partition key,
            //  in practice this means that yes, you have transactions, but only when you write documents 
            //  of the same collection and partition key using a stored procedure.

            // So, let's start by grouping the documents to persist by collection and partition key:
            var groupedDocs = documents.Where(i => i.State.IsDirty()).
                                        GroupBy(i => new { i.CollectionId, i.Document.partitionKey });

            foreach (var collectionDocs in groupedDocs )
            {
                var collectionId = collectionDocs.Key.CollectionId;
                var partitionKey = collectionDocs.Key.partitionKey;

                // For each combination of collection and partition key we build a list with:
                //      * doc state
                //      * doc contents
                //      * a link to the existing document when we are updating/deleting an existing one (null otherwise)
                // This is needed because the statements to execute inside the procedure are different
                //  for new and updated/deleted docs.
                // These properties are encapsulated in class PersistenceProcedureDocProperties
                var docProperties = collectionDocs.Select(doc => 
                    new PersistenceProcedureDocProperties(doc, _databaseId, collectionId)).ToList();

                // As said, stored procedure are scoped by collection and partition key:
                var storedProcedureUri = UriFactory.CreateStoredProcedureUri(_databaseId, collectionId, _persistenceSpId);
                var options = new RequestOptions { PartitionKey = new PartitionKey(partitionKey)};

                // It may happen that Cosmos DB decides to kill the stored procedure if it takes too long.
                //  It's a soft termination and the SP can still return a value,
                //  but you need to take this possibility into account.
                // When it's an SP writing many docs, the typical implementation pattern is 
                //  returning an integer indicating how many docs the SP has managed to write before being terminated,
                //  then retrying with the remaining docs.
                var total = docProperties.Count;
                var totalPersisted = 0;
                while (totalPersisted < total)
                {
                    var result = await this.Client.ExecuteStoredProcedureAsync<int>(
                        storedProcedureUri, options, docProperties);
                    var persisted = result.Response;
                    totalPersisted += persisted;
                    if (totalPersisted < total)
                    {
                        // Some were not persisted: take them from the list and retry:
                        docProperties = docProperties.GetRange(persisted, docProperties.Count - persisted);
                    }
                }
            }

            return true;
        }

        internal async Task<List<R>> LoadManyAsync<T,R>(string collection, string partitionKey,
            IEnumerable<DbDocumentCondition<T>> conditions,
            System.Linq.Expressions.Expression<Func<DbDocument<T>, R>> select,
            int maxItemsCount = -1)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseId, collection);

            var query = this.Client.CreateDocumentQuery<DbDocument<T>>(uri,
                    new FeedOptions { MaxItemCount = maxItemsCount, PartitionKey = new PartitionKey(partitionKey)}).
                    AsQueryable();

            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    query = query.Where(condition);
                }
            }

            List<R> results = new List<R>();
            var docQuery = query.Select(select).AsDocumentQuery();
            while (docQuery.HasMoreResults)
            {
                results.AddRange(await docQuery.ExecuteNextAsync<R>());
            }

            return results;
        }

        internal async Task<T> LoadAsync<T>(string collection, string partitionKey, string id) where T : class
        {
            var sql = $"SELECT VALUE c.docContents FROM c WHERE c.partitionKey = '{partitionKey}' AND c.id = '{id}'";

            return await QuerySingleItemAsync<T>(sql, collection, partitionKey);
        }

        private async Task<T> QuerySingleItemAsync<T>(string sql, string collection, string partitionKey) where T : class
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseId, collection);
            var query = this.Client.CreateDocumentQuery<T>(uri, sql,
                new FeedOptions { MaxItemCount = 1, PartitionKey = new PartitionKey(partitionKey)}).
                AsDocumentQuery();

            var result = await query.ExecuteNextAsync<T>();

            return result.FirstOrDefault();
        }

        private DocumentClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (_clientLock)
                    {
                        if (_client == null)
                        {
                            _client = new DocumentClient(_endpoint, _authKey,
                                // For efficiency, use Tcp and Direct connection mode
                                // https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips
                                new ConnectionPolicy
                                {
                                    ConnectionMode = ConnectionMode.Direct,
                                    ConnectionProtocol = Protocol.Tcp
                                });
                        }
                    }
                }

                return _client;
            }
        }

        /// <summary>
        /// This class encapsulates the properties submitted to the stored procedure responsible
        /// for persisting changes to Cosmos DB.
        /// It also takes care of serializing the objects so we can control serialization in detail.
        /// </summary>
        private class PersistenceProcedureDocProperties
        {
            public string state { get; }
            public string document { get; }
            public string documentLink { get; }

            // To ensure type information isn't lost when serializing/deserializing we use the
            //  JSON setting TypeNameHandling.Auto.
            // Otherwise deserializing a property Order would only retrieve properties of the base Order class
            //  and not those of the derived classes, for example.
            private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            public PersistenceProcedureDocProperties(TrackedDbDocument doc,
                string databaseId, string collectionId)
            {
                this.state = doc.State.ToString();
                this.document = JsonConvert.SerializeObject(doc.Document, _jsonSettings);
                this.documentLink = doc.State == DocumentState.Added ?
                            null :
                            UriFactory.CreateDocumentUri(databaseId, collectionId, doc.Document.id).ToString();
            }
        }

        #region DISPOSE
        ~CosmosDBGateway()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                {
                    lock (_clientLock)
                    {
                        if (_client != null)
                        {
                            _client.Dispose();
                            _client = null;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
