using System;
using System.Collections.Generic;

namespace nucoris.queries.PatientDetails
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<string> Allergies { get; set; }

        public AdmissionDetails ActiveAdmission { get; set; }
        public List<Admission> Admissions { get; set; }
    }
}