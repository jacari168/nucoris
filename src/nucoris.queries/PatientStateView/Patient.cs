using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace nucoris.queries.PatientStateView
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] // to serialize as string
        public PatientAdmissionState State { get; set; }

        public List<Admission> Admissions { get; set; }

        // To facilitate case insensitive searches we duplicate the PID in uppercase:
        public string UpperMrn { get; set; }
        public string UpperGivenName { get; set; }
        public string UpperFamilyName { get; set; }
    }
}