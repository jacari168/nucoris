using System;
using System.Collections.Generic;
using System.Text;

namespace nucoris.domain.tests
{
    public static class TestUtilities
    {
        public static Patient CreateSamplePatient()
        {
            return Patient.NewPatient("mrn", "givenName", "familyName", new DateTime(1919, 1, 28));
        }

        public static Medication CreateSampleMedication()
        {
            return new Medication("Diazepam");
        }

        public static MedicationPrescription CreateSamplePrescription(Admission admission = null)
        {
            if (admission == null)
            {
                Patient patient = TestUtilities.CreateSamplePatient();
                admission = patient.Admit(DateTime.UtcNow);
            }

            var medication = TestUtilities.CreateSampleMedication();
            Unit mg = new Unit("mg");
            var dose = new Dose(2, mg);
            var frequency = new Frequency("Q8H");
            var prescriber = new User(Guid.NewGuid(), "username", "givenName", "familyName");
            var prescribedAt = DateTime.UtcNow;
            var startAt = DateTime.UtcNow;
            var endAt = DateTime.UtcNow.AddDays(5);

            return admission.Prescribe(medication, dose, frequency, startAt, endAt, prescriber, prescribedAt);
        }
    }
}
