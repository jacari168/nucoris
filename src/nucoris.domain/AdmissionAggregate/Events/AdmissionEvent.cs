namespace nucoris.domain
{
    public abstract class AdmissionEvent : DomainEvent
    {
        public Admission Admission { get; }

        public AdmissionEvent(Admission admission) : base(admission.PatientIdentity)
        {
            Admission = admission;
        }
    }
}