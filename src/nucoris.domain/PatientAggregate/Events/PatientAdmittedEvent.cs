namespace nucoris.domain
{
    public class PatientAdmittedEvent : PatientEvent
    {
        public override string Description => "Patient admitted";
        public Admission Admission { get; }

        public PatientAdmittedEvent(Patient patient, Admission admission) : base(patient)
        {
            Admission = admission;
        }
    }
}