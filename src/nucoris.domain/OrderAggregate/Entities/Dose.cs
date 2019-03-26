using Ardalis.GuardClauses;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class Dose : ValueObject
    {
        public decimal Amount { get; }
        public Unit Unit { get; }

        public Dose(decimal amount, Unit unit)
        {
            Guard.Against.Condition(amount <= 0, "A dose cannot be 0");
            Guard.Against.Null(unit, "Dose unit");

            Amount = amount;
            Unit = unit;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Unit;
        }
    }
}