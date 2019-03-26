using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nucoris.application.commands
{
    public class AddAllergiesCommand : MediatR.IRequest<CommandResult>
    {
        public Guid PatientId { get; }
        public List<string> Allergies { get; }

        public AddAllergiesCommand(Guid patientId, IEnumerable<string> allergies)
        {
            Guard.Against.Null(allergies, "No allergy specified");

            this.PatientId = patientId;
            this.Allergies = allergies.ToList();
        }
    }
}
