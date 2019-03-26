using System;

namespace nucoris.domain
{
    public abstract class Entity
    {
        public Guid Id { get; }

        protected Entity(Guid id)
        {
            this.Id = id;
        }

        public override bool Equals(object obj)
        {
            Entity other = obj as Entity;

            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (this.Id == default(Guid) || other.Id == default(Guid))
                return false;

            return this.Id == other.Id;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

    }
}
