namespace nucoris.domain
{
    public class RadiologyExamRequestedEvent : OrderEvent
    {
        public override string Description => $"Requested radiology exam '{this.Order.Description}'";

        public RadiologyExamRequestedEvent(RadiologyExam order)
                : base(order)
        {
        }
    }
}