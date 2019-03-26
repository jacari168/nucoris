
namespace nucoris.application.interfaces
{
    /// <summary>
    /// The architecture of nucoris allows the implementation of two types of event handlers:
    /// - synchronous: handlers that process a domain event before the entity changes that raised it are persisted.
    /// - asynchronous: handlers that process events after the request has been fulfilled and a response returned to the client.
    ///                 These are implemented by a loop composed of the following elements:
    ///                    1) A function consumes Cosmos DB change feed to persist events into a service bus called "application"
    ///                    2) Another function subscribes to messages in that bus and posts them to the web api "api/applicationEvents"
    ///                    3) The ApplicationEventsController uses the mediator to send the received events to their handler, if any.
    /// To allow the mediator to differentiate between both types of handlers, we introduced the generic class below that encapsulates
    ///     domain events. Asynchronous handlers implement handlers to specific types of this class.
    /// </summary>
    public class DeferredDomainEvent<T> : MediatR.INotification where T : nucoris.domain.DomainEvent
    {
        public T Event { get; }

        public DeferredDomainEvent(T domainEvent)
        {
            this.Event = domainEvent;
        }
    }
}
