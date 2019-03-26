using System;

namespace nucoris.webapp.Models
{
    public class UserVM
    {
        public UserVM() { }

        public UserVM(domain.User user)
        {
            this.Id = user.Id;
            this.GivenName = user.GivenName;
            this.FamilyName = user.FamilyName;
            this.DisplayName = domain.NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
            this.Username = user.Username;
        }

        public Guid Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
    }
}
