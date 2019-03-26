namespace nucoris.domain
{
    public class OrderAssignedEvent : OrderEvent
    {
        public override string Description => $"Assigned '{this.Order.Description}' to '{this.AssignedTo.DisplayName}'";

        // AssignedTo could be deduced from order properties,
        //  but it's worth specifying it separately for expressiveness and to facilitate queries
        public User AssignedTo { get; }

        public OrderAssignedEvent(Order order, User assignedTo) : base(order)
        {
            AssignedTo = assignedTo;
        }
    }
}