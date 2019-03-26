namespace nucoris.domain
{
    public class MedicationPrescribedEvent : OrderEvent
    {
        public override string Description => $"Prescribed '{this.Order.Description}'";

        public MedicationPrescribedEvent(MedicationPrescription order)
                : base(order)
        {
        }
    }
}