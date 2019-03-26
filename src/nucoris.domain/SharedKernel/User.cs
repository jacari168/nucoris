using Ardalis.GuardClauses;
using Newtonsoft.Json;
using System;

namespace nucoris.domain
{
    public class User : Entity, IReferencePersistable
    {
        public string Username { get; }
        public string GivenName { get; }
        public string FamilyName { get; }

        [JsonIgnore]
        public string DisplayName { get; }

        public User(Guid id, string username, string givenName, string familyName) : base(id)
        {
            Guard.Against.NullOrEmpty(username, "Username");
            Guard.Against.NullOrEmpty(givenName, "Given name");
            Guard.Against.NullOrEmpty(familyName, "Family name");

            Username = username;
            GivenName = givenName;
            FamilyName = familyName;
            DisplayName = NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
        }
    }
}