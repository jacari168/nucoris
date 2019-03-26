using System;

namespace nucoris.domain
{
    public static class NameUtilities
    {
        public static string BuildDisplayName(string givenName, string familyName)
        {
            if (String.IsNullOrWhiteSpace(familyName))
            {
                return givenName;
            }
            else
            {
                if (String.IsNullOrWhiteSpace(givenName))
                {
                    return familyName.ToUpper();
                }
                else
                {
                    return $"{familyName.ToUpper()}, {givenName}";
                }
            }
        }
    }
}
