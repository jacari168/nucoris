using System;

namespace nucoris.persistence
{
    /// <summary>
    /// This interface defines the methods a class building configuration details should implement.
    /// </summary>
    internal interface IDbTypeConfigurationBuilder
    {
        DbTypeConfiguration BuildFor(object o);
        DbTypeConfiguration BuildForId(Type type, string id);
        DbTypeConfiguration BuildForType(Type type);
        string BuildDocType(Type type);
    }
}
