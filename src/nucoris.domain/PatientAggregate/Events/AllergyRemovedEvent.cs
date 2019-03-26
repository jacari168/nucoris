namespace nucoris.domain
{
    public class AllergyRemovedEvent : PatientEvent
    {
        public override string Description => $"Allergy '{this.Allergy.Name}' removed";

        public Allergy Allergy { get; }

        public AllergyRemovedEvent(Patient patient, Allergy allergy) : base(patient)
        {
            Allergy = allergy;
        }
    }
}