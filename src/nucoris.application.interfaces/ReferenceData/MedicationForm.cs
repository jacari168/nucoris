using Newtonsoft.Json;
using nucoris.domain;
using System;

namespace nucoris.application.interfaces.ReferenceData
{
    /// <summary>
    /// This class represents a combination of a medication and a dose.
    /// It is defined in the application layer because it is a convenience class
    /// used in UI and in the persistence layer only. 
    /// In the domain medication and dose are currently separated entities.
    /// </summary>
    public class MedicationForm : IReferencePersistable
    {
        public Guid Id { get; }
        public Medication Medication { get; }
        public Dose Dose { get; }

        [JsonIgnore]
        public string DisplayName { get; }

        public MedicationForm(Guid id, Medication medication, Dose dose)
        {
            Id = id;
            Medication = medication;
            Dose = dose;
            DisplayName = $"{medication.Name} {dose.Amount:F1} {dose.Unit.Name}";
        }
    }
}
