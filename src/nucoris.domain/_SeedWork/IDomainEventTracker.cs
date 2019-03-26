using System.Collections.Generic;

namespace nucoris.domain
{
    /// <summary>
    /// This interface identifies classes that track the events they raise while executing the implementation of a command.
    /// When the persistence layer is based on Entity Framework it's quite easy to track them at each entity level
    ///     as seen, for example, in: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/domain-events-design-implementation
    /// Otherwise, when you are sure that every request is handled by a single thread you can use a static class 
    ///     as seen in Udi Dahan's suggestion in: http://udidahan.com/2009/06/14/domain-events-salvation/
    /// 
    /// In our case we have to do some extra work since we neither use EF nor can assume single-thread requests:
    /// - We assume only aggregate roots raise events
    /// - We make aggregate roots implement this interface
    /// - We only store aggregate roots
    /// - At commit time we gather the events from the aggregate roots
    /// </summary>
    public interface IDomainEventTracker
    {
        IReadOnlyCollection<DomainEvent> Events { get; }
        void AddEvent(DomainEvent newEvent);
        void ClearEvents();
    }
}
