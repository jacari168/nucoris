namespace nucoris.domain
{
    public class OrderStartedEvent : OrderEvent
    {
        public override string Description => $"Started '{this.Order.Description}'";

        public OrderStartedEvent(Order order) : base(order)
        {
        }
    }
}