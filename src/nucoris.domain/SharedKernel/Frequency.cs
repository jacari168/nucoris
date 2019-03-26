using Ardalis.GuardClauses;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class Frequency : ValueObject
    {
        public string Name { get; }

        public Frequency(string name)
        {
            Guard.Against.NullOrEmpty(name, "Frequency name");

            Name = name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name.ToUpper();
        }
    }
}