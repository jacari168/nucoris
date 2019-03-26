using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace nucoris.domain.tests
{
    [TestClass]
    public class MedicationTests
    {
        [TestMethod]
        public void Medication_WhenPrescribedOnAdmittedPatient_OrderCreated()
        {
            // A prescription on an active admission should result in a new order with status "Created"
            //  and all its properties properly set

            Patient patient = TestUtilities.CreateSamplePatient();
            var admission = patient.Admit(DateTime.UtcNow);

            var medication = TestUtilities.CreateSampleMedication();
            var dose = new Dose( 5, new Unit("mg"));
            var frequency = new Frequency("Q8H");
            var startAt = DateTime.UtcNow;
            var endAt = startAt.AddDays(5);
            var prescriber = new User(Guid.NewGuid(), "username", "givenName", "familyName");
            var prescribedAt = DateTime.UtcNow;

            admission.Prescribe(medication, dose, frequency, startAt, endAt, prescriber, prescribedAt);

            admission.Orders.ShouldHaveSingleItem();
            var prescription = admission.Orders.First() as MedicationPrescription;
            prescription.ShouldNotBeNull();
            prescription.State.ShouldBe(OrderState.Created);
            prescription.Medication.ShouldBe(medication);
            prescription.StartAt.ShouldBe(startAt);
            prescription.EndAt.ShouldBe(endAt);
            prescription.Frequency.ShouldBe(frequency);
            prescription.OrderedAt.ShouldBe(prescribedAt);
            prescription.OrderedBy.ShouldBe(prescriber);
            prescription.PatientIdentity.Id.ShouldBe(patient.Id);
            prescription.AdmissionId.ShouldBe(admission.Id);
        }

        [TestMethod]
        public void Medication_WhenPrescribedOnAdmittedPatient_MedicationPrescribedEventRaised()
        {
            Patient patient = TestUtilities.CreateSamplePatient();
            var admission = patient.Admit(DateTime.UtcNow);
            var prescription = TestUtilities.CreateSamplePrescription(admission);

            var eventRaised = admission.Events.FirstOrDefault(e => e is MedicationPrescribedEvent) as MedicationPrescribedEvent;
            eventRaised.ShouldNotBeNull();
            eventRaised.Order.ShouldBe(prescription);
        }

        [TestMethod]
        public void Medication_WhenPrescribedOnInactiveAdmission_OrderRejected()
        {
            // Trying to prescribe an order on a non-admitted patient is rejected with an exception.

            var patient = TestUtilities.CreateSamplePatient();
            var admission = patient.Admit(DateTime.UtcNow.AddDays(-1));
            admission.End(DateTime.UtcNow);

            var medication = TestUtilities.CreateSampleMedication();
            var dose = new Dose(5, new Unit("mg"));
            var frequency = new Frequency("Q8H");
            var startAt = DateTime.UtcNow;
            var endAt = startAt.AddDays(5);
            var prescriber = new User(Guid.NewGuid(), "username", "givenName", "familyName");
            var prescribedAt = DateTime.UtcNow;

            Should.Throw<ApplicationException>(() => admission.Prescribe(medication, dose, frequency, startAt, endAt, prescriber, prescribedAt));
        }

        [TestMethod]
        public void Medication_WhenAdministered_OrderStartedAndMedicationAdministered()
        {
            var prescription = TestUtilities.CreateSamplePrescription();
            var administrator = new User(Guid.NewGuid(), "username", "givenName", "familyName");
            var administeredAt = DateTime.UtcNow;
            var administration = prescription.Administer(administrator, administeredAt);

            // Note that when order administered for first time, status of order changes to InProgress.
            prescription.State.ShouldBe(OrderState.InProgress);
            administration.AdministeredBy.ShouldBe(administrator);
            administration.AdministeredAt.ShouldBe(administeredAt);
        }

        [TestMethod]
        public void Medication_WhenAdministered_MedicationAdministeredEventRaised()
        {
            var prescription = TestUtilities.CreateSamplePrescription();
            var administrator = new User(Guid.NewGuid(), "username", "givenName", "familyName");
            var administeredAt = DateTime.UtcNow;
            var administration = prescription.Administer(administrator, administeredAt);

            // Note that when order administered for first time, status of order changes to InProgress.
            var startedEventRaised = prescription.Events.FirstOrDefault(e => e is OrderStartedEvent) as OrderStartedEvent;
            startedEventRaised.ShouldNotBeNull();
            startedEventRaised.Order.ShouldBe(prescription);

            var administeredEventRaised = prescription.Events.FirstOrDefault(e => e is MedicationAdministeredEvent) as MedicationAdministeredEvent;
            administeredEventRaised.ShouldNotBeNull();
            administeredEventRaised.Order.ShouldBe(prescription);
            administeredEventRaised.Administration.ShouldBe(administration);
        }
    }
}
