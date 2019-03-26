namespace nucoris.domain
{
    /// <summary>
    /// As explained in the comments to IPersistable, nucoris data is persisted in Cosmos DB document DB. 
    /// In Cosmos DB every document (object) to be persisted must have:
    ///     - a property called "Id"
    ///     - a property that can be configured as partition key of the Cosmos collection
    /// Having an Id property it's easy, all entities have it.
    /// For patient-related data we use as partition key a patient identifier to ensure 
    ///     a reasonable distribution of values
    ///     and that all documents of a patient can be efficiently retrieved and stored.
    /// So, all entities related to patient data to be persisted shall have an Id and a patient identifier, 
    ///     encapsulated here in class PatientIdentity.
    /// </summary>
    public interface IPatientPersistable : IPersistable
    {
        PatientIdentity PatientIdentity { get; }
    }
}