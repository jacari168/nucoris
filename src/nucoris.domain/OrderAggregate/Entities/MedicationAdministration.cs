using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class MedicationAdministration : ValueObject
    {
        public User AdministeredBy { get; }
        public DateTime AdministeredAt { get; }

        public MedicationAdministration(User administeredBy, DateTime administeredAt)
        {
            Guard.Against.Null(administeredBy, "Administering user");

            AdministeredBy = administeredBy;
            AdministeredAt = administeredAt;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AdministeredBy;
            yield return AdministeredAt;
        }
    }
}