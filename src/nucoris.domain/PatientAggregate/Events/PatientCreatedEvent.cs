namespace nucoris.domain
{
    public class PatientCreatedEvent : PatientEvent
    {
        public override string Description => "Patient created";

        public PatientCreatedEvent(Patient patient) : base(patient)
        {
        }
    }
}