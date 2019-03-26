using Ardalis.GuardClauses;
using Newtonsoft.Json;
using System;

namespace nucoris.domain
{
    public class RadiologyExam : Order
    {
        public RadiologyProcedure Procedure { get; }
        [JsonProperty]
        public DateTime? PerformedAt { get; private set; }

        public RadiologyExam(Guid guid, PatientIdentity patientIdentity, Guid admissionId, RadiologyProcedure procedure, User orderedBy, DateTime orderedAt):
            base(guid, patientIdentity, admissionId, orderedBy, orderedAt)
        {
            Guard.Against.Null(procedure, "Radiology procedure");

            this.Procedure = procedure;

            base.Description = $"{this.Procedure.Modality.ToString()} {this.Procedure.BodyPart}";
        }
    }
}