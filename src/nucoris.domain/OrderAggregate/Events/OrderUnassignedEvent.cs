namespace nucoris.domain
{
    public class OrderUnassignedEvent : OrderEvent
    {
        public override string Description => $"Unassigned '{this.Order.Description}'";
        public User PreviouslyAssignedTo { get; }

        public OrderUnassignedEvent(Order order, User previousAssignee) : base(order)
        {
            PreviouslyAssignedTo = previousAssignee;
        }
    }
}