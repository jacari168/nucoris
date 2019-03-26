namespace nucoris.domain
{
    public class MedicationAdministeredEvent : OrderEvent
    {
        public override string Description => $"Administered '{this.Order.Description}'";
        public MedicationAdministration Administration { get; }

        public MedicationAdministeredEvent(MedicationPrescription order, MedicationAdministration newAdministration)
                : base(order)
        {
            Administration = newAdministration;
        }
    }
}