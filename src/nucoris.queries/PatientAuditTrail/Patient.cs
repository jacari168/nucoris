using System;
using System.Collections.Generic;

namespace nucoris.queries.PatientAuditTrail
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }

        public List<AuditTrailItem> Items { get; set; }
    }
}