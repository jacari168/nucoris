using System;
using System.Collections.Generic;
using System.Text;

namespace nucoris.application.commands
{
    public class CreatePatientCommand : MediatR.IRequest<CommandResultWithCreatedEntityId>
    {
        public string Mrn { get; }
        public string GivenName { get; }
        public string FamilyName { get; }
        public DateTime? DateOfBirth { get; }

        public CreatePatientCommand(string mrn, string givenName, string familyName, DateTime? dateOfBirth)
        {
            Mrn = mrn;
            GivenName = givenName;
            FamilyName = familyName;
            DateOfBirth = dateOfBirth;
        }
    }
}
