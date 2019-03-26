namespace nucoris.domain
{
    public abstract class OrderEvent : DomainEvent
    {
        public Order Order { get; }

        public OrderEvent(Order order) : base(order.PatientIdentity)
        {
            Order = order;
        }
    }
}