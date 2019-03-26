using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nucoris.domain
{
    public class Patient : AggregateRoot
    {
        // This property could be used in the final application to flag patients whose identity needs verification
        //  (e.g. showing them in a separate list)
        public bool HasSystemGeneratedMrn { get; }

        public string GivenName { get; private set; }
        public string FamilyName { get; private set; }
        public string DisplayName { get; private set; }
        public DateTime? DateOfBirth { get; private set; }

        // We use a typical DDD pattern to protect collection members of aggregate root.
        // We serialize allergies together with patient,
        //  but we'll have admissions stored each in a separate document ("table")
        private readonly List<Allergy> _allergies = new List<Allergy>();
        public IReadOnlyCollection<Allergy> Allergies => _allergies.AsReadOnly();
        private readonly List<Admission> _admissions = new List<Admission>();
        [Newtonsoft.Json.JsonIgnore]
        public IReadOnlyCollection<Admission> Admissions => _admissions.AsReadOnly();


        // This is the only entity where we have a factory method because Patient is the "root" of the whole domain
        //  and has no parent entity.
        // All other entities are created in methods of a parent entity (e.g. patient.Admit, admission.Prescribe)
        public static Patient NewPatient(string mrn, string givenName, string familyName, DateTime? dob)
        {
            // For the purposes of this prototype it's enough to initialize the MRN to a GUID if missing,
            //  but in the final application a more sophisticated mechanism should be used (GUID are difficult for users to deal with).
            Guid id = Guid.NewGuid();
            bool isMrnGeneratedBySystem = false;
            if (String.IsNullOrWhiteSpace(mrn))
            {
                mrn = id.ToString();
                isMrnGeneratedBySystem = true;
            }

            var patient = new Patient( new PatientIdentity(id, mrn), givenName, familyName, dob, isMrnGeneratedBySystem);

            patient.AddEvent(new PatientCreatedEvent(patient));

            return patient;
        }

        [Newtonsoft.Json.JsonConstructor] // JsonConstructor attribute is needed because we've marked the constructor as private
        private Patient(PatientIdentity patientIdentity, string givenName, string familyName, DateTime? dateOfBirth, 
                bool hasSystemGeneratedMrn, 
                List<Allergy> allergies = null, List<Admission> admissions = null)
            : base(patientIdentity.Id, patientIdentity)
        {
            Guard.Against.Null(patientIdentity, "PatientIdentity");
            Guard.Against.NullOrWhiteSpace(patientIdentity.Mrn, "MRN");

            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.DisplayName = NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
            this.DateOfBirth = dateOfBirth;
            this.HasSystemGeneratedMrn = hasSystemGeneratedMrn;
            if (allergies != null) this._allergies.AddRange(allergies);
            if (admissions != null) this._admissions.AddRange(admissions);
        }

        public void UpdateDemographics(string givenName, string familyName, DateTime? dob)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.DisplayName = NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
            this.DateOfBirth = dob;

            base.AddEvent(new PatientDemogsUpdatedEvent(this));
        }

        public void Add(Allergy allergy)
        {
            Guard.Against.Null(allergy, "Allergy");

            // Avoid duplicates:
            if (!_allergies.Any(existing => existing == allergy) )
            {
                _allergies.Add(allergy);
                base.AddEvent(new AllergyAddedEvent(this, allergy));
            }
        }

        public void Remove(Allergy allergy)
        {
            if( allergy != null )
            {
                if (_allergies.Remove(allergy))
                {
                    base.AddEvent(new AllergyRemovedEvent(this, allergy));
                }
            }
        }

        public Admission Admit(DateTime actualAdmissionTime)
        {
            var currentAdmission = GetCurrentAdmission();
            Guard.Against.Condition(currentAdmission != null, "Patient is already admitted.");
            Guard.Against.OutOfRange(actualAdmissionTime, "Admission time must be between patient's date of birth and current time",
                            this.DateOfBirth != null ? this.DateOfBirth.Value : DateTime.MinValue, DateTime.UtcNow);

            var admission = new Admission(Guid.NewGuid(), this.PatientIdentity);
            _admissions.Add(admission);

            base.AddEvent(new PatientAdmittedEvent(this, admission));

            admission.Start(actualAdmissionTime);

            return admission;
        }

        private Admission GetCurrentAdmission()
        {
            var currentAdmission = _admissions.FirstOrDefault(i => i.IsActive);

            return currentAdmission;
        }
    }
}
