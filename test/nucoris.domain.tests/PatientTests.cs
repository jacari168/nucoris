using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace nucoris.domain.tests
{
    [TestClass]
    public class PatientTests
    {
        [TestMethod]
        public void Patient_WhenCreated_PIDSetCorrectly()
        {
            var patient = Patient.NewPatient("mrn", "givenName", "familyName", new DateTime(2019, 1, 28));
            patient.ShouldNotBeNull();
            patient.PatientIdentity.Mrn.ShouldBe("mrn");
            patient.GivenName.ShouldBe("givenName");
            patient.FamilyName.ShouldBe("familyName");
            patient.DateOfBirth.ShouldBe(new DateTime(2019, 1, 28));
        }

        [TestMethod]
        public void Patient_WhenCreated_PatientCreatedEventRaised()
        {
            var patient = Patient.NewPatient("mrn", "givenName", "familyName", new DateTime(2019, 1, 28));
            var eventRaised = patient.Events.FirstOrDefault(e => e is PatientCreatedEvent) as PatientCreatedEvent;
            eventRaised.ShouldNotBeNull();
            eventRaised.Patient.ShouldBe(patient);
        }

        [TestMethod]
        public void Patient_CanBeCreatedWithJustMRN()
        {
            var patient = Patient.NewPatient("mrn", "givenName", "familyName", new DateTime(2019, 1, 28));
            patient.ShouldNotBeNull();
            patient = Patient.NewPatient("mrn", null, null, null);
            patient.ShouldNotBeNull();
        }

        [TestMethod]
        public void Patient_WhenMRNNotSpecified_MRNSetToSystemId()
        {
            var patient = Patient.NewPatient("", "givenName", "familyName", new DateTime(2019, 1, 28));
            patient.HasSystemGeneratedMrn.ShouldBeTrue();
            patient.PatientIdentity.Mrn.ShouldNotBeEmpty();

            patient = Patient.NewPatient(null, "givenName", "familyName", new DateTime(2019, 1, 28));
            patient.HasSystemGeneratedMrn.ShouldBeTrue();
            patient.PatientIdentity.Mrn.ShouldNotBeEmpty();
        }

        [TestMethod]
        public void Patient_WhenDemographicsUpdated_PIDSetCorrectly()
        {
            var patient = Patient.NewPatient("mrn", "givenName", "familyName", new DateTime(2019, 1, 28));
            patient.UpdateDemographics("newGivenName", "newFamilyName", new DateTime(2019, 1, 29));
            patient.PatientIdentity.Mrn.ShouldBe("mrn");
            patient.GivenName.ShouldBe("newGivenName");
            patient.FamilyName.ShouldBe("newFamilyName");
            patient.DateOfBirth.ShouldBe(new DateTime(2019, 1, 29));
        }

        [TestMethod]
        public void Patient_WhenAdmitted_ActiveAdmissionCreated()
        {
            var patient = TestUtilities.CreateSamplePatient();
            var admissionTime = DateTime.UtcNow;
            var admission = patient.Admit(admissionTime);

            admission.ShouldNotBeNull();
            admission.Started.ShouldBe(admissionTime);
            admission.Ended.ShouldBeNull();
            admission.IsActive.ShouldBe(true);
            admission.PatientIdentity.Id.ShouldBe(patient.Id);
        }

        [TestMethod]
        public void Patient_WhenAdmitted_PatientAdmittedEventRaised()
        {
            var patient = TestUtilities.CreateSamplePatient();
            var admissionTime = DateTime.UtcNow;
            var admission = patient.Admit(admissionTime);
            var eventRaised = patient.Events.FirstOrDefault(e => e is PatientAdmittedEvent) as PatientAdmittedEvent;
            eventRaised.ShouldNotBeNull();
            eventRaised.Patient.ShouldBe(patient);
            eventRaised.Admission.ShouldBe(admission);
        }

        [TestMethod]
        public void Patient_WhenAdmitted_CannotBeAdmittedAgain()
        {
            var patient = TestUtilities.CreateSamplePatient();
            var admissionTime = DateTime.UtcNow;
            var admission = patient.Admit(admissionTime);

            Should.Throw<ApplicationException>(() => patient.Admit(DateTime.UtcNow));
        }

        [TestMethod]
        public void Patient_WhenAllergyDocumented_AllergyAddedToList()
        {
            var patient = TestUtilities.CreateSamplePatient();

            var allergy = new Allergy("Nuts");
            patient.Add(allergy);

            patient.Allergies.ShouldHaveSingleItem();
            patient.Allergies.ShouldContain(allergy);
        }

        [TestMethod]
        public void Patient_WhenAllergyDocumented_AllergyAddedEventRaised()
        {
            var patient = TestUtilities.CreateSamplePatient();
            var allergy = new Allergy("Nuts");
            patient.Add(allergy);
            var eventRaised = patient.Events.FirstOrDefault(e => e is AllergyAddedEvent) as AllergyAddedEvent;
            eventRaised.ShouldNotBeNull();
            eventRaised.Patient.ShouldBe(patient);
            eventRaised.Allergy.ShouldBe(allergy);
        }

        [TestMethod]
        public void Patient_WhenDuplicateAllergyDocumented_AllergyIsIgnored()
        {
            var patient = TestUtilities.CreateSamplePatient();

            var allergy = new Allergy("Nuts");
            patient.Add(allergy);

            var duplicate = new Allergy("NUTS");
            patient.Add(duplicate);

            // The following checks assume that Allergy is a ValueObject,
            //  and allergies with same name are semantically the same object.
            patient.Allergies.ShouldHaveSingleItem();
            patient.Allergies.ShouldContain(duplicate);
            patient.Allergies.ShouldContain(allergy);
        }
    }
}
