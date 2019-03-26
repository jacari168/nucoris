using nucoris.domain;
using System;

namespace nucoris.persistence
{
    /// <summary>
    ///     This factory creates the default implementation of IDbSessionConfiguration.
    /// That is, it encapsulates a description of how all nucoris persisted objects are stored in Cosmos DB:
    /// their collection id, how is partition key constructed, and the value of our custom property docType.
    ///     As you can deduce from the code below, we store all objects in a single collection: this is a common
    /// practice in Cosmos DB to reduce costs, and it has no negative effect on performance 
    /// since performance is mostly influenced by the distribution of partition keys.
    ///     To manage having all documents (persisted objects) in the same collection 
    /// we assign partition keys according to these rules:
    ///     - There is a partition per patient, whose key follows the pattern "P_" + patient's guid.
    /// All objects of a patient, including its events, are stored in this partition.
    /// 
    ///     - There is a partition per each reference data type (e.g. one for users, one for medications, etc).
    /// Its key follows the pattern "R_" + type name (e.g. "R_Medication").
    /// In each of such partitions we expect to find a single document for each reference item (e.g. "Diazepam", "Propofol", etc.).
    /// 
    ///     - There is a partition per each materialized view, containing items that populate worklists in the UI
    /// Its key follows the pattern "V_" + view/worklist name (e.g. "V_ActiveOrdersView").
    /// 
    /// Note: Users are currently handled as reference data, 
    /// but in the final application its profile will grow and include its clinical profile, rights, security audit, etc.
    /// Then there may be worth having a partition per user, whose key follows the pattern "U_" + user's guid.
    /// 
    /// </summary>
    public static class DbSessionConfigurationFactory
    {
        private const string _defaultCollectionId = "nucorisCol";
        private static readonly Func<string, string> _getPartitionKeyFromPatientId =
                (string patientId) => $"P_{patientId}";

        public static IDbSessionConfiguration New()
        {
            DbSessionConfiguration config = new DbSessionConfiguration();

            // In this prototype the domain model has a single bounded context
            //  where all entities are related to patient care, and the ones we persist
            //  (Patient, Admission, Order-derived) have AggregateRoot as base class and implement IPatientPersistable.
            // So we can take advantage of this to just define a rule for all of them:
            config.AddConfigurationBuilder<IPatientPersistable>(
                _defaultCollectionId,
                partitionKeyItemBuilder: (item) => _getPartitionKeyFromPatientId(item.PatientIdentity.Id.ToString()),
                docTypeBuilder: (type) => type.Name,
                partitionKeyItemIdBuilder: (stringId) => _getPartitionKeyFromPatientId(stringId)
                );

            // However, for Order-derived we use "Order" as docType:
            //  (configuration of classes take precedence over interfaces, so this one will prevail over IPatientPersistable)
            config.AddConfigurationBuilder<Order>(
                _defaultCollectionId,
                partitionKeyItemBuilder: (item) => _getPartitionKeyFromPatientId(item.PatientIdentity.Id.ToString()),
                docTypeBuilder: (type) => "Order",
                partitionKeyItemIdBuilder: (stringId) => _getPartitionKeyFromPatientId(stringId)
                );

            // Events are also stored together with patient data,
            //  and they also implement IPatientPersistable, 
            //  but they all have "Event" as document type, so let's define a specific config from them
            config.AddConfigurationBuilder<DomainEvent>(
                _defaultCollectionId,
                partitionKeyItemBuilder: (item) => _getPartitionKeyFromPatientId(item.PatientIdentity.Id.ToString()),
                docTypeBuilder: (item) => "Event",
                partitionKeyItemIdBuilder: (stringId) => _getPartitionKeyFromPatientId(stringId)
                );

            // All reference data (including users) are stored in partition "R_" + type name
            config.AddConfigurationBuilder<IReferencePersistable>(
                _defaultCollectionId,
                partitionKeyTypeBuilder: (type) => $"R_{type.Name}",
                docTypeBuilder: (type) => "Reference"
                );


            // For application worklists displaying data from multiple patients
            //  we define a materialized view, and all candidate items are stored
            //  together in the same partition, to enable queries with different criteria within a single partition.
            // Each materialized view has a distinct partition whose id starts with "V_" followed by the view name.
            config.AddConfigurationBuilder<queries.PatientStateView.PatientStateViewItem>(
                _defaultCollectionId,
                partitionKeyTypeBuilder: (type) => $"V_PatientStateView",
                docTypeBuilder: (item) => "View"
                );
            config.AddConfigurationBuilder<queries.ActiveOrdersView.ActiveOrdersViewItem>(
                _defaultCollectionId,
                partitionKeyTypeBuilder: (type) => $"V_ActiveOrdersView",
                docTypeBuilder: (item) => "View"
                );

            return config;
        }
    }
}
