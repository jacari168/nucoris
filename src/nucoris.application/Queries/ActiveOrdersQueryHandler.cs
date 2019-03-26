using nucoris.application.interfaces;
using nucoris.queries.ActiveOrdersView;

namespace nucoris.application.queries
{
    public class ActiveOrdersQueryHandler : MaterializedViewQueryHandler<ActiveOrdersQuery, ActiveOrdersViewItem>
    {
        public ActiveOrdersQueryHandler(
            IMaterializedViewRepository<ActiveOrdersViewItem, IMaterializedViewSpecification<ActiveOrdersViewItem>> queryRepository)
            : base(queryRepository)
        {
        }
    }


}
