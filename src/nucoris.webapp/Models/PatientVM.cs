using System;
using System.Collections.Generic;
using System.Linq;
using nucoris.queries.PatientDetails;
using nucoris.queries.PatientStateView;

namespace nucoris.webapp.Models
{
    public class PatientVM
    {
        public PatientVM() {}

        public PatientVM(PatientStateViewItem queryItem)
        {
            var pat = queryItem.QueryPatient;
            this.Id = pat.Id;
            this.Mrn = pat.Mrn;
            this.GivenName = pat.GivenName;
            this.FamilyName = pat.FamilyName;
            this.DisplayName = domain.NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
            this.DateOfBirth = pat.DateOfBirth;

            var adm = pat.Admissions.FirstOrDefault(a => a.IsActive);
            if(adm != null)
            {
                this.ActiveAdmission = new AdmissionVM()
                {
                    Id = adm.Id,
                    Started = adm.Started,
                    Ended = adm.Ended
                };
            };
        }

        public PatientVM(queries.PatientDetails.Patient pat)
        {
            this.Id = pat.Id;
            this.Mrn = pat.Mrn;
            this.GivenName = pat.GivenName;
            this.FamilyName = pat.FamilyName;
            this.DisplayName = pat.DisplayName;
            this.DateOfBirth = pat.DateOfBirth;
            this.Allergies = pat.Allergies;
            this.Admissions = pat.Admissions?.Select(a => CreateAdmission(a)).ToList();

            if (pat.ActiveAdmission != null)
            {
                this.ActiveAdmission = CreateAdmission(pat.ActiveAdmission, pat);
            };
        }

        public PatientVM(queries.ActiveOrdersView.ActiveOrdersViewItem orderViewItem)
        {
            var order = orderViewItem.QueryOrder;
            this.Id = order.PatientId;
            this.Mrn = order.Mrn;
            this.GivenName = order.GivenName;
            this.FamilyName = order.FamilyName;
            this.DisplayName = domain.NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
            this.ActiveAdmission = new AdmissionVM() { Id = order.AdmissionId };
        }

        public Guid Id { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<string> Allergies { get; set; }
        public List<AdmissionVM> Admissions { get; set; }
        public AdmissionVM ActiveAdmission { get; set; }

        private AdmissionVM CreateAdmission(queries.PatientDetails.Admission admission)
        {
            return new AdmissionVM()
            {
                Id = admission.Id,
                Started = admission.Started,
                Ended = admission.Ended,
                Orders = new List<OrderVM>()
            };
        }

        private AdmissionVM CreateAdmission(queries.PatientDetails.AdmissionDetails admissionDetails,
                                            queries.PatientDetails.Patient patient)
        {
            var admission = CreateAdmission(admissionDetails);
            admission.Orders = admissionDetails.Orders?.Select(o => new OrderVM(o, admissionDetails, patient)).ToList();

            return admission;
        }
    }
}
