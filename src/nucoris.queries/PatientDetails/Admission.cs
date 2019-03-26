using System;

namespace nucoris.queries.PatientDetails
{
    public class Admission
    {
        public Guid Id { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
    }
}