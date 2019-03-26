using Newtonsoft.Json;
using nucoris.domain;
using System;

namespace nucoris.application.interfaces.ReferenceData
{
    /// <summary>
    /// This class is a wrapper over domain's medication frequency.
    /// In the domain frequency it's just a value object with no id,
    ///     but in the presentation and persistence layer it's convenient
    ///     to define a list of frequencies and to identify them with a Guid.
    /// On the other hand, in the prescription bounded context it's fine to call it just "Frequency",
    ///     but outside of the domain frequency is an ambiguous term
    ///     and it's better to qualify it with "Medication" or "Administration", for example.
    /// </summary>
    public class AdministrationFrequency : IReferencePersistable
    {
        public Guid Id { get; }
        public Frequency Frequency { get; }

        [JsonIgnore]
        public string DisplayName { get; }

        public AdministrationFrequency(Guid id, Frequency frequency)
        {
            Id = id;
            Frequency = frequency;
            DisplayName = frequency.Name;
        }
    }
}
