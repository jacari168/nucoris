using System;

namespace nucoris.queries.PatientAuditTrail
{
    public class AuditTrailItem
    {
        public string Description { get; set; }
        public string DoneBy { get; set; }
        public Guid? DoneById { get; set; }
        public DateTime DoneAt { get; set; }
    }
}