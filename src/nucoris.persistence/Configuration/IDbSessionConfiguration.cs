using System;
using nucoris.application.interfaces;

namespace nucoris.persistence
{
    /// <summary>
    /// This interface defines the methods that a class encapsulating the details of 
    /// how each domain type is stored in Cosmos DB (collection id and partition key)
    /// should implement.
    /// See DbSessionConfigurationFactory for further explanation.
    /// </summary>
    public interface IDbSessionConfiguration
    {
        DbTypeConfiguration GetDBConfiguration<T>(T item) where T : class;
        DbTypeConfiguration GetDBConfiguration<T>() where T : class;
        DbTypeConfiguration GetDBConfiguration(Type type, string itemId);
        string GetDBDocType<T>();
    }
}
