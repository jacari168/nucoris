using Ardalis.GuardClauses;
using System.Collections.Generic;

namespace nucoris.domain
{
    public enum Modality
    {
        CR,
        CT,
        MR
    }

    public class RadiologyProcedure : ValueObject
    {
        public Modality Modality { get; }
        public string BodyPart { get; }

        public RadiologyProcedure(Modality modality, string bodyPart)
        {
            Guard.Against.NullOrEmpty(bodyPart, "Body part");

            Modality = modality;
            BodyPart = bodyPart;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Modality;
            yield return BodyPart.ToUpper();
        }
    }
}