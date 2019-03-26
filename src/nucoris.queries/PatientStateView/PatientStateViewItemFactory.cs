using System;
using System.Linq;
using nucoris.domain;

namespace nucoris.queries.PatientStateView
{
    public static class PatientStateViewItemFactory
    {
        public static PatientStateViewItem FromDomain(domain.Patient patient)
        {
            return new PatientStateViewItem()
            {
                Id = patient.Id,
                QueryPatient = 
                    new Patient()
                    {
                        Id = patient.Id,
                        Admissions = patient.Admissions.Select(domainAdmission => CreateAdmission(domainAdmission)).ToList(),
                        DateOfBirth = patient.DateOfBirth,
                        FamilyName = patient.FamilyName,
                        GivenName = patient.GivenName,
                        Mrn = patient.PatientIdentity.Mrn,
                        State = (patient.Admissions.Any(admission => admission.IsActive) ?
                                    PatientAdmissionState.Admitted : PatientAdmissionState.Discharged),
                        UpperFamilyName = patient.FamilyName?.ToUpper(),
                        UpperGivenName = patient.GivenName?.ToUpper(),
                        UpperMrn = patient.PatientIdentity.Mrn?.ToUpper()
                    }
            };
        }

        public static PatientStateViewItem FromExisting(PatientStateViewItem item, domain.Admission admission)
        {
            if (item.QueryPatient.Admissions == null) item.QueryPatient.Admissions = new System.Collections.Generic.List<Admission>();

            var existingAdmission = item.QueryPatient.Admissions.FirstOrDefault(i => i.Id == admission.Id);
            if (existingAdmission != null) item.QueryPatient.Admissions.Remove(existingAdmission);

            item.QueryPatient.Admissions.Add(CreateAdmission(admission));

            item.QueryPatient.State = (item.QueryPatient.Admissions.Any(i => i.IsActive) ?
                PatientAdmissionState.Admitted : PatientAdmissionState.Discharged);

            return item;
        }

        private static Admission CreateAdmission(domain.Admission domainAdmission)
        {
            return new Admission()
            {
                Id = domainAdmission.Id,
                Started = domainAdmission.Started,
                Ended = domainAdmission.Ended,
                IsActive = domainAdmission.IsActive
            };
        }
    }
}
