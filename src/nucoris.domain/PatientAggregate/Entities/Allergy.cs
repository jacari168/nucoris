using Ardalis.GuardClauses;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class Allergy : ValueObject
    {
        public string Name { get; }

        public Allergy(string name)
        {
            Guard.Against.NullOrEmpty(name, "Allergy name");

            Name = name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name.ToUpper();
        }
    }
}
