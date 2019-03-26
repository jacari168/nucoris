using Ardalis.GuardClauses;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class Medication : ValueObject
    {
        public string Name { get; }

        public Medication(string name)
        {
            Guard.Against.NullOrEmpty(name, "Medication name");

            Name = name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name.ToUpper();
        }
    }
}