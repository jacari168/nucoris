namespace nucoris.domain
{
    public class OrderCompletedEvent : OrderEvent
    {
        public override string Description => $"Completed '{this.Order.Description}'";

        public OrderCompletedEvent(Order order) : base(order)
        {
        }
    }
}