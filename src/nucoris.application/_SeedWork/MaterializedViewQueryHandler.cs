using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.queries
{
    /// <summary>
    /// This is the base class for all query handlers.
    /// </summary>
    /// <typeparam name="Q">A class derived from Query defining the query for type I</typeparam>
    /// <typeparam name="I">A class derived from QueryItem</typeparam>
    public abstract class MaterializedViewQueryHandler<Q,I> : IRequestHandler<Q, List<I>> 
            where I : MaterializedViewItem
            where Q : MaterializedViewQuery<I>
    {
        private readonly IMaterializedViewRepository<I, IMaterializedViewSpecification<I>> _queryRepository;

        public MaterializedViewQueryHandler(
            IMaterializedViewRepository<I, IMaterializedViewSpecification<I>> queryRepository)
        {
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
        }

        public async Task<List<I>> Handle(Q query, CancellationToken cancellationToken)
        {
            Guard.Against.Null(query, nameof(query));
            Guard.Against.Null(query.Specification, "Query specification");

            return await _queryRepository.GetManyAsync(query.Specification);
        }
    }


}
