using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nucoris.domain
{
    /// <summary>
    /// Implementation adapted and enhanced from:
    /// https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
    /// https://enterprisecraftsmanship.com/2017/08/28/value-object-a-better-implementation/
    /// 
    /// Derived classes shall implement GetEqualityComponents() as explained in the second link.
    /// An alternative approach would be to use reflection to get all derived members, but GetEqualityComponents
    ///     lets derived classes decide which members and how should be taken into account.
    /// For example, I've used it in some cases to implement case-insensitive equality comparison of string members.
    /// </summary>
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            ValueObject other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
             .Select(x => x != null ? x.GetHashCode() : 0)
             .Aggregate((x, y) => x ^ y);
        }

        // Overriding operator== and != is necessary to ensure Equals is eventually called.
        //  Otherwise these operators perform just a ReferenceEquals by default.
        public static bool operator==(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        public static bool operator!=(ValueObject left, ValueObject right)
        {
            return !(left == right);
        }        
    }
}
