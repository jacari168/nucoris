namespace nucoris.domain
{
    public class AdmissionStartedEvent : AdmissionEvent
    {
        public override string Description => "Admission started";

        public AdmissionStartedEvent(Admission admission) : base(admission)
        {
        }
    }
}