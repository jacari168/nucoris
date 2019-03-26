namespace nucoris.domain
{
    public abstract class PatientEvent : DomainEvent
    {
        public Patient Patient { get; }

        public PatientEvent(Patient patient) : base(patient.PatientIdentity)
        {
            Patient = patient;
        }
    }
}