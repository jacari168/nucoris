namespace nucoris.domain
{
    public class OrderCancelledEvent : OrderEvent
    {
        public override string Description => $"Cancelled '{this.Order.Description}'";

        public OrderCancelledEvent(Order order) : base(order)
        {
        }
    }
}