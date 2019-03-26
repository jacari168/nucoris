using nucoris.application.interfaces;
using nucoris.queries.ActiveOrdersView;

namespace nucoris.application.queries
{
    public class ActiveOrdersQuery : MaterializedViewQuery<ActiveOrdersViewItem>
    {
        public ActiveOrdersQuery(IMaterializedViewSpecification<ActiveOrdersViewItem> specification)
            : base(specification)
        {
        }
    }
}
