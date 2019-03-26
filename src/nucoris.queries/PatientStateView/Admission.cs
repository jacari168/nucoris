using System;

namespace nucoris.queries.PatientStateView
{
    public class Admission
    {
        public Guid Id { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public bool IsActive { get; set; }
    }
}