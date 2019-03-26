using nucoris.application.interfaces;
using System.Collections.Generic;

namespace nucoris.application.interfaces
{
    /// <summary>
    /// This interface defines the method a MaterializedViewSpecification should implement.
    /// In nucoris, a MaterializedViewSpecification is an implementation of the Specification pattern
    /// that generates a collection of LINQ expressions that can be applied to filter
    /// the results of database queries on materialized views (data projections from raw patient data).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMaterializedViewSpecification<T> 
    {
        IEnumerable<DbDocumentCondition<T>> AsLinqExpressions();
    }
}