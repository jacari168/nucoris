using nucoris.application.interfaces;
using System.Collections.Generic;

namespace nucoris.application.queries
{
    /// <summary>
    /// This is the base class for queries on materialized views.
    /// </summary>
    /// <typeparam name="I">A class derived from QueryItem</typeparam>
    public abstract class MaterializedViewQuery<I> : MediatR.IRequest<List<I>>
        where I : MaterializedViewItem
    {
        public IMaterializedViewSpecification<I> Specification { get; }

        public MaterializedViewQuery(IMaterializedViewSpecification<I> specification)
        {
            this.Specification = specification;
        }
    }
}
