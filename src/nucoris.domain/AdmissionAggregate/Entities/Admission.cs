using Ardalis.GuardClauses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace nucoris.domain
{
    public class Admission : AggregateRoot
    {
        // Without this attribute, dates are not deserialized because "set" is not public
        [JsonProperty]
        public DateTime Started { get; private set; }
        [JsonProperty] 
        public DateTime? Ended { get; private set; }

        private readonly List<Order> _orders = new List<Order>();
        [JsonIgnore]
        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

        public Admission(Guid id, PatientIdentity patientIdentity, List<Order> orders = null) : base(id, patientIdentity)
        {
            if (orders != null) this._orders.AddRange(orders);
        }

        public bool IsActive => (Ended == null);

        public void Start(DateTime started)
        {
            this.Started = started;

            base.AddEvent(new AdmissionStartedEvent(this));
        }

        public void End(DateTime endAt)
        {
            VerifyTimeIsWithinOpenAdmission(endAt);

            // Note that we pass end time as a parameter to let the application layer (if needed)
            //  document actions that have already occurred.
            // It also helps with testing :-)
            this.Ended = endAt;

            base.AddEvent(new AdmissionEndedEvent(this));
        }

        public MedicationPrescription Prescribe(Medication medication, Dose dose, 
            Frequency frequency, DateTime startAt, DateTime endAt,
            User prescribedBy, DateTime prescribedAt)
        {
            VerifyTimeIsWithinOpenAdmission(prescribedAt);

            var order = new MedicationPrescription(Guid.NewGuid(), this.PatientIdentity, this.Id,
                medication, dose, frequency, startAt, endAt, prescribedBy, prescribedAt);

            _orders.Add(order);

            base.AddEvent(new MedicationPrescribedEvent(order));

            return order;
        }

        public RadiologyExam Request(RadiologyProcedure procedure, User by, DateTime requestedAt)
        {
            VerifyTimeIsWithinOpenAdmission(requestedAt);

            var order = new RadiologyExam(Guid.NewGuid(), this.PatientIdentity, this.Id, procedure, by, requestedAt);

            _orders.Add(order);

            base.AddEvent(new RadiologyExamRequestedEvent(order));

            return order;
        }

        private void VerifyTimeIsWithinOpenAdmission(DateTime actionTime)
        {
            Guard.Against.Condition( ! this.IsActive, "This admission is currently not active.");
            Guard.Against.OutOfRange(actionTime, "Action time must be between admission time and current time",
                            this.Started, DateTime.UtcNow);
        }

    }
}