using System;
using System.Collections.Generic;
using System.Linq;
using nucoris.queries.PatientDetails;
using nucoris.queries.PatientStateView;

namespace nucoris.webapp.Models
{
    public class PatientAuditTrailVM
    {
        public PatientAuditTrailVM() {}

        public PatientAuditTrailVM(PatientStateViewItem queryItem)
        {
            var pat = queryItem.QueryPatient;
            this.Id = pat.Id;
            this.Mrn = pat.Mrn;
            this.GivenName = pat.GivenName;
            this.FamilyName = pat.FamilyName;
            this.DisplayName = nucoris.domain.NameUtilities.BuildDisplayName(pat.GivenName, pat.FamilyName);
        }

        public Guid Id { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public List<AuditTrailItemVM> Items { get; set; }
    }
}
