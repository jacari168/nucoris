using System;
using System.Collections.Generic;

namespace nucoris.domain
{
    /// <summary>
    /// This class contains the identifiers that can be used to identify a patient.
    /// Ironically we derive it from ValueObject because two sets of identifiers with the same values
    ///     represent the same object.
    /// </summary>
    public class PatientIdentity : ValueObject
    {
        public Guid Id { get; private set; }
        public string Mrn { get; private set; }

        public PatientIdentity(Guid id, string mrn)
        {
            Id = id;
            Mrn = mrn;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Mrn.ToUpper();
        }
    }
}
