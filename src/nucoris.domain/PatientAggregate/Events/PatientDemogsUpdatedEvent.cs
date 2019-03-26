namespace nucoris.domain
{
    public class PatientDemogsUpdatedEvent : PatientEvent
    {
        public override string Description => "Patient demographic data updated";

        public PatientDemogsUpdatedEvent(Patient patient) : base(patient)
        {
        }
    }
}