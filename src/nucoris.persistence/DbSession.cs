using Ardalis.GuardClauses;
using Newtonsoft.Json;
using nucoris.application.interfaces;
using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace nucoris.persistence
{
    /// <summary>
    /// This class implements nucoris' Unit of Work.
    /// Specifically, it's a variation of the "unit of work controller" pattern
    /// described by Martin Fowler in "Patterns of Enterprise Application Architecture".
    /// (pages 187-188 of its 2003 edition).
    /// 
    /// Repositories interact with this class in two ways:
    /// - by asking it to Load entities from the database
    /// - by Registering new or modified entities
    /// 
    /// Command handlers interact with this class to get tracked entities and to commit changes.
    /// 
    /// So, it's similar to Entity Framework's DbContext except that you must explicitly register both new and modified entities.
    /// In EF you only have to explicitly register new entities (usually by calling DbSet's Add method).
    /// It would be technically feasible in this class to automatically track modified entities like EF does, 
    /// but I defer this to a future version :-)
    /// </summary>
    public class DbSession : application.interfaces.IUnitOfWork
    {
        private readonly List<TrackedDbDocument> _documents = new List<TrackedDbDocument>();
        private readonly CosmosDBGateway _cosmosDBGateway;
        private readonly IDbSessionConfiguration _dbConfig;
        private readonly IUserIdentityProvider _userIdentityProvider;
        private User _currentUser;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public DbSession(CosmosDBGateway cosmosDBGateway, IDbSessionConfiguration dbConfig, IUserIdentityProvider userIdentityProvider)
        {
            _cosmosDBGateway = cosmosDBGateway;
            _dbConfig = dbConfig;
            _userIdentityProvider = userIdentityProvider;
        }

        #region IUnitOfWork METHODS
        public User CurrentUser
        {
            get
            {
                if(_currentUser == null)
                {
                    // This is a special method implemented here because it's used by DbSession to determine userId from username logged on.
                    // We can't rely on UserRepository because it depends on DbSession and would create a circular dependency.
                    var userTask = GetUserFromUsernameAsync(_userIdentityProvider.CurrentUsername);
                    _currentUser = userTask.Result ?? 
                        throw new UnauthorizedAccessException($"Cannot determine nucoris user for username {_userIdentityProvider.CurrentUsername}");
                }

                return _currentUser;
            }
        }

        /// <summary>
        /// Gets persistable entities tracked by this unit of work.
        /// </summary>
        /// <returns>Read only collection of tracked entities</returns>
        public IReadOnlyCollection<IPersistable> GetTrackedEntities()
        {
            // We do not expect that in the same session two different threads will call the public interface methods,
            //   but just in case we perform a lock before accessing the list of documents.
            lock (_documents)
            {
                return _documents.Select(i => i.Item).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Commits dirty (new, modified, deleted) entities.
        /// </summary>
        /// <returns>True if successful</returns>
        public async Task<bool> CommitAsync()
        {
            List<TrackedDbDocument> editedDocs;

            // We create a temporary list with the currently dirty documents 
            //  in case a different thread unexpectedly added more documents while we are committing the current ones.
            lock (_documents)
            {
                editedDocs = _documents.Where(i => i.State.IsDirty()).ToList();
            }

            bool success = await _cosmosDBGateway.CommitAsync(editedDocs);

            if (success)
            {
                lock (_documents)
                {
                    // We expect that the session will be disposed after committing,
                    //  so resetting the document state should not be needed, 
                    //  but we do it anyway just in case...:
                    _documents.RemoveAll(i => i.State == DocumentState.Deleted && editedDocs.Contains(i));
                    editedDocs.ForEach(i => i.State = DocumentState.Loaded);
                }
            }

            return success;
        }
        #endregion

        #region LOAD SINGLE ITEM
        /// <summary>
        /// Load a single patient data item.
        /// </summary>
        /// <typeparam name="T">A persistable patient-related entity implementing IPatientPersistable</typeparam>
        /// <param name="patientId">Patient Guid</param>
        /// <param name="itemId">Child entity Guid (patient Guid if you are loading the patient)</param>
        /// <returns>Patient [child] entity</returns>
        internal async Task<T> LoadAsync<T>(Guid patientId, Guid itemId)
                where T : class, IPatientPersistable
        {
            var config = _dbConfig.GetDBConfiguration(typeof(T), patientId.ToString());

            return await LoadAsync<T>(config.CollectionId, config.PartitionKey, itemId.ToString());
        }

        /// <summary>
        /// Load a single non-patient data item
        /// (by "non-patient data" we mean data not associated to a specific patient, such as medication).
        /// </summary>
        /// <typeparam name="T">A persistable data type</typeparam>
        /// <param name="itemId">Guid of the item.</param>
        /// <returns>The item</returns>
        internal async Task<T> LoadAsync<T>(Guid itemId)
                where T : class, IPersistable
        {
            return await LoadAsync<T>(itemId.ToString());
        }

        /// <summary>
        /// Load a single non-patient data item
        /// (by "non-patient data" we mean data not associated to a specific patient, such as medication).
        /// We assume non-patientData has a partitionKey not depending on itemId
        /// </summary>
        /// <typeparam name="T">A persistable data type</typeparam>
        /// <param name="itemId">Id of the item.</param>
        /// <returns>The item</returns>
        internal async Task<T> LoadAsync<T>(string itemId)
            where T : class, IPersistable
        {
            var config = _dbConfig.GetDBConfiguration<T>();

            return await LoadAsync<T>(config.CollectionId, config.PartitionKey, itemId);
        }

        private async Task<T> LoadAsync<T>(string collection, string partitionKey, string id)
            where T : class
        {
            T item = await _cosmosDBGateway.LoadAsync<T>(collection, partitionKey, id);

            if (item != null && item is IPersistable)
            {
                // We register all loaded items so that we can track changes to them
                // But we need to get the docType from configuration:
                var docType = _dbConfig.GetDBDocType<T>();
                RegisterInternal(item as IPersistable, collection, partitionKey, docType, DocumentState.Loaded);
            }

            return item;
        }
        #endregion

        #region LOAD MANY
        /// <summary>
        /// Method to get patient's children of a single specific type that is an aggregate root.
        /// Note: to get all children derived of an abstract class such as DomainEvent or Order use instead LoadManyDerivedAsync
        /// </summary>
        /// <typeparam name="T">Specific entity type. Should be an aggregate root</typeparam>
        /// <param name="patientId">Guid of patient</param>
        /// <param name="whereConditions">Enumerable of Linq conditions on the entity type</param>
        /// <returns>List of entities read from the database</returns>
        internal async Task<List<T>> LoadManyAsync<T>(
            Guid patientId,
            IEnumerable<DbDocumentCondition<T>> whereConditions)
            where T : AggregateRoot
        {
            var config = _dbConfig.GetDBConfiguration(typeof(T), patientId.ToString());

            var items = await _cosmosDBGateway.LoadManyAsync(config.CollectionId, config.PartitionKey,
                    whereConditions, select: (doc) => doc.docContents);

            items.ForEach((item) => RegisterInternal(item,
                       config.CollectionId, config.PartitionKey, config.DocType, DocumentState.Loaded));

            return items;
        }

        /// <summary>
        /// Gets patient's children of a given type and/or its derived types.
        /// For example, to get all DomainEvent or Order instances of a patient. 
        /// Or even all their children by passing IPatientPersistable as type.
        /// It is advisable to pass a condition on docType if you know that the instances you are interested in
        /// all have a common docType (e.g. "Order" for all orders).
        /// </summary>
        /// <typeparam name="T">Entity type. Should be an entity implementing IPatientPersistable.
        /// It could be a base class. For example, specify DomainEvent to retrieve all events</typeparam>
        /// <param name="patientId">Guid of patient</param>
        /// <param name="whereConditions">Enumerable of conditions</param>
        /// <returns>List of objects read from the database of the specified type or derived</returns>
        internal async Task<List<T>> LoadManyDerivedAsync<T>(
            Guid patientId,
            IEnumerable<DbDocumentCondition<T>> whereConditions)
            where T : IPatientPersistable
        {
            // The implementation of this method is a bit tricky because we may be requested
            //  to load items of different types, so we can't use the standard query methods
            //  from the ComosDB Document .NET Api that automatically deserialize the read documents
            //  into a specific type.
            // What we do is to load all docs as dynamic objects,
            //  deduce the type of each of them from their docSubType property,
            //  take the serialized value from their docContents property
            //  and deserialize it using the type information just deduced.

            var config = _dbConfig.GetDBConfiguration(typeof(T), patientId.ToString());

            var docs = await _cosmosDBGateway.LoadManyAsync<T,dynamic>(config.CollectionId, config.PartitionKey, 
                    whereConditions, (doc) => doc);

            List<T> items = new List<T>();

            Assembly domainTypesAssembly = Assembly.GetAssembly(typeof(T));

            foreach (dynamic doc in docs)
            {
                var typeFullname = doc.docSubType as string;
                var type = domainTypesAssembly.GetType(typeFullname);
                var serializedDoc = doc.docContents.ToString();
                var item = JsonConvert.DeserializeObject(serializedDoc, type, _jsonSettings);
                if (item is T)
                {
                    items.Add(item);
                    if (item is IPersistable)
                    {
                        RegisterInternal(item as IPersistable,
                            config.CollectionId, config.PartitionKey, doc.docType as string, DocumentState.Loaded);
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Method to get many non-patient items (e.g. reference data or materialized view items),
        /// which we assume have a database configuration (partitionKey) not depending on the value of an id such as patientId.
        /// </summary>
        /// <typeparam name="T">Non-patient persistable data type.</typeparam>
        /// <param name="whereConditions">Enumerable of Linq conditions</param>
        /// <returns>List of items read from the database</returns>
        internal async Task<List<T>> LoadManyAsync<T>(
            IEnumerable<DbDocumentCondition<T>> whereConditions)
            where T : class, IPersistable
        {
            var config = _dbConfig.GetDBConfiguration<T>();

            var items = await _cosmosDBGateway.LoadManyAsync(config.CollectionId, config.PartitionKey, 
                    whereConditions, select: (doc) => doc.docContents);

            items.ForEach( (item) => RegisterInternal(item,
                        config.CollectionId, config.PartitionKey, config.DocType, DocumentState.Loaded) );

            return items;
        }

        // This is a special method implemented here because it's used by DbSession to determine userId from username logged on.
        // We can't rely on UserRepository because it depends on DbSession and would create a circular dependency.
        internal async Task<User> GetUserFromUsernameAsync(string username)
        {
            var matchingUsers = await LoadManyAsync(
                new List<DbDocumentCondition<User>>()
                {
                    new DbDocumentCondition<User>((doc) => doc.docContents.Username == username)
                });

            if (matchingUsers != null && matchingUsers.Count > 0)
            {
                return matchingUsers[0];
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region REGISTER
        internal void Register(IPersistable itemToPersist)
        {
            // By default we assume it's a new object and pass Added as state.
            //  RegisterInternal below we'll check if it's already tracked and if so change it to Modified. 
            // This works because all existing objects that are edited by command handlers
            //  must have previously been loaded are tracked by the same session.
            // If this changed in the future we would need to request callers to specify
            //  whether they are adding a new object or updating an existing one.
            Register(itemToPersist, DocumentState.Added);
        }

        internal void RegisterForDeletion(IPersistable itemToPersist)
        {
            Register(itemToPersist, DocumentState.Deleted);
        }

        private void Register(IPersistable itemToPersist, DocumentState state)
        {
            Guard.Against.Null(itemToPersist, nameof(itemToPersist));
            Guard.Against.Condition(itemToPersist.Id == default(Guid), "Item doesn't have a valid Id");

            var config = _dbConfig.GetDBConfiguration(itemToPersist);
            RegisterInternal(itemToPersist, config.CollectionId, config.PartitionKey, config.DocType, state);
        }

        private void RegisterInternal(IPersistable itemToPersist, 
                string collection, string partitionKey, string docType, 
                DocumentState requestedState)
        {
            lock (_documents)
            {
                //  If there is already a document containing an item with the same id and type,
                //      check whether it's the same object: if so, just update its state.
                // (the extra condition on type is required because some query items have the same id as domain entities)
                var existing = _documents.FirstOrDefault(i => i.Item.Id == itemToPersist.Id 
                                && i.Item.GetType().Name == itemToPersist.GetType().Name);

                DocumentState newState = GetNewState(requestedState, existing);

                if (existing != null && ReferenceEquals(existing.Item, itemToPersist))
                {
                    existing.State = newState;
                }
                else
                {
                    if (existing != null)
                    {
                        // Same id but not the same object: remove and add again, assuming the latest one is more up-to-date.
                        _documents.Remove(existing);
                    }

                    _documents.Add(new TrackedDbDocument(itemToPersist, collection, partitionKey, docType, newState));
                }
            }
        }

        private static DocumentState GetNewState(DocumentState requestedState, TrackedDbDocument existingDoc)
        {
            DocumentState newState = requestedState; // let's start by assuming the requested state is ok.

            // If the doc is already there and we are not trying to delete it,
            //  let's make sure the new state is consistent with the existing one:
            if (existingDoc != null && newState != DocumentState.Deleted)
            {
                if (existingDoc.State == DocumentState.Loaded)
                {
                    // If trying to register as Added a doc that already exists, the state should actually be Modified.
                    // Otherwise we'll use the requested state.
                    if (requestedState == DocumentState.Added) newState = DocumentState.Modified;
                }
                else
                {
                    // Respect existing state (Modified/Added), since it's an item waiting to be persisted.
                    //  and its state has been previously verified.
                    newState = existingDoc.State;
                }
            }

            return newState;
        }
        #endregion

    }
}
