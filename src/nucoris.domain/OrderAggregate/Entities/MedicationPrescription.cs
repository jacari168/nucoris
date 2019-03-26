using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class MedicationPrescription : Order
    {
        public Medication Medication { get; }
        public Dose Dose { get; }
        public Frequency Frequency { get; }
        public DateTime StartAt { get; }
        public DateTime EndAt { get; }

        // DDD pattern to protect collection members of aggregate root:
        private readonly List<MedicationAdministration> _administrations = new List<MedicationAdministration>();
        public IReadOnlyCollection<MedicationAdministration> Administrations => _administrations.AsReadOnly();

        public MedicationPrescription(Guid id, PatientIdentity patientIdentity, Guid admissionId,
            Medication medication, Dose dose, 
            Frequency frequency, DateTime startAt, DateTime endAt,
            User orderedBy, DateTime orderedAt,
            List<MedicationAdministration> administrations = null) : base(id, patientIdentity, admissionId, orderedBy, orderedAt)
        {
            Guard.Against.Null(medication, "Medication");
            Guard.Against.Null(dose, "Dose");
            Guard.Against.Null(frequency, "Frequency");

            this.Medication = medication;
            this.Dose = dose;
            this.Frequency = frequency;
            this.StartAt = startAt;
            this.EndAt = endAt;
            if (administrations != null) this._administrations.AddRange(administrations);

            base.Description = $"{this.Medication.Name} {this.Dose.Amount:F1} {this.Dose.Unit.Name} {this.Frequency.Name}";
        }

        public MedicationAdministration Administer(User by, DateTime at)
        {
            Guard.Against.OutOfValidStates(State, OrderState.Created, OrderState.Ready, OrderState.InProgress);
            Guard.Against.OutOfRange(at, "Medication administration time", this.StartAt, DateTime.UtcNow);

            if (this.State != OrderState.InProgress)
            {
                Start();
            }

            // Note that we pass time as a parameter to let the application layer (if needed)
            //  document actions that have already occurred.
            // It's pretty common when taking care of patients.
            var newAdministration = new MedicationAdministration(by, at);
            _administrations.Add(newAdministration);

            base.AddEvent(new MedicationAdministeredEvent(this, newAdministration));

            return newAdministration;
        }

    }
}