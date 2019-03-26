namespace nucoris.domain
{
    public class AllergyAddedEvent : PatientEvent
    {
        public override string Description => $"Allergy '{this.Allergy.Name}' added";

        public Allergy Allergy { get; }

        public AllergyAddedEvent(Patient patient, Allergy allergy) : base(patient)
        {
            Allergy = allergy;
        }
    }
}