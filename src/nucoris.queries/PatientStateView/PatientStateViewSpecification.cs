using Newtonsoft.Json;
using nucoris.application.interfaces;
using System;
using System.Collections.Generic;

namespace nucoris.queries.PatientStateView
{
    public class PatientStateViewSpecification : IMaterializedViewSpecification<PatientStateViewItem>
    {
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PatientAdmissionState State { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }

        public PatientStateViewSpecification(PatientAdmissionState state) : this(state, null, null, null) { }

        public PatientStateViewSpecification(string mrn) : this(PatientAdmissionState.Any, mrn, null, null) { }

        public PatientStateViewSpecification(PatientAdmissionState state, string mrn, string givenName, string familyName)
        {
            this.State = state;
            this.Mrn = mrn;
            this.GivenName = givenName;
            this.FamilyName = familyName;
        }


        public IEnumerable<DbDocumentCondition<PatientStateViewItem>> AsLinqExpressions()
        {
            var conditions = new List<DbDocumentCondition<PatientStateViewItem>>();

            if( this.State != PatientAdmissionState.Any)
            {
                conditions.Add(new DbDocumentCondition<PatientStateViewItem>(doc => doc.docContents.QueryPatient.State == this.State));
            }

            if( ! String.IsNullOrWhiteSpace(this.Mrn))
            {
                conditions.Add(new DbDocumentCondition<PatientStateViewItem>(doc => doc.docContents.QueryPatient.UpperMrn == this.Mrn.ToUpper()));
            }

            if (!String.IsNullOrWhiteSpace(this.GivenName))
            {
                conditions.Add(new DbDocumentCondition<PatientStateViewItem>(doc => doc.docContents.QueryPatient.UpperGivenName == this.GivenName.ToUpper()));
            }

            if (!String.IsNullOrWhiteSpace(this.FamilyName))
            {
                conditions.Add(new DbDocumentCondition<PatientStateViewItem>(doc => doc.docContents.QueryPatient.UpperFamilyName == this.FamilyName.ToUpper()));
            }

            return conditions;
        }
    }
}