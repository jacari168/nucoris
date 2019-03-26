using nucoris.application.interfaces;

namespace nucoris.queries.ActiveOrdersView
{
    public class ActiveOrdersViewItem : MaterializedViewItem
    {
        public Order QueryOrder {get;set;}
    }
}
