namespace nucoris.domain
{
    /// <summary>
    /// As César de la Torre et al. write in their Microservices Architecture Guide, 
    /// "There are always constraints and trade-offs in the different database technologies, 
    ///     so you will not be able to have the same model for relational or NoSQL databases. [...]
    ///     You really have to design your model with an understanding of how the data is going to be used in each particular database."
    /// https://aka.ms/microservicesebook, p. 253 in .NET Core 2.2 edition
    /// Well, nucoris data is persisted in Cosmos DB document DB. 
    /// In Cosmos DB every document (object) to be persisted must have:
    ///     - a property called "Id"
    ///     - a property that can be configured as partition key of the Cosmos collection
    /// The interface below defines the Id property. Partition key is deferred to other interfaces or 
    ///     to configuration in the persistence layer.
    /// </summary>
    public interface IPersistable
    {
        System.Guid Id { get; }
    }
}