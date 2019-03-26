using Ardalis.GuardClauses;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class Unit : ValueObject
    {
        public Unit(string name)
        {
            Guard.Against.NullOrEmpty(name, "Unit name");
            Name = name;
        }

        public string Name { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name.ToUpper();
        }
    }
}