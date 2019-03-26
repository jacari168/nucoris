namespace nucoris.domain
{
    public class AdmissionEndedEvent : AdmissionEvent
    {
        public override string Description => "Admission ended";

        public AdmissionEndedEvent(Admission admission) : base(admission)
        {
        }
    }
}