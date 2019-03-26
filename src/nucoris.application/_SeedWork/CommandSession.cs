using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class CommandSession : ICommandSession
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MediatR.IMediator _mediator;
        private readonly IEventRepository _eventRepository;

        public CommandSession(IUnitOfWork unitOfWork, MediatR.IMediator mediator, IEventRepository eventRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException("Unit of work");
            _mediator = mediator ?? throw new ArgumentNullException("Mediator");
            _eventRepository = eventRepository ?? throw new ArgumentNullException("Event repository");
        }

        public async Task<bool> CommitAsync()
        {
            // The implementation of the command handler may generate domain events.
            // We are interested to collect and store them as part of the same transaction:

            List<DomainEvent> events = await DispatchEventsAsync();

            _eventRepository.StoreMany(events);

            return await _unitOfWork.CommitAsync();
        }

        private async Task<List<DomainEvent>> DispatchEventsAsync()
        {
            // To dispath the events we have to call the mediator.
            // However, it may happen that an event dispatcher invokes more domain methods
            //  which in turn generate more events...
            // So we loop through the events until no more undispatched events remain.
            List<DomainEvent> events = new List<DomainEvent>();
            List<DomainEvent> newEvents;

            do
            {
                newEvents = await GetAndDispatchEventsAsync();
                events.AddRange(newEvents);
            } while (newEvents.Count > 0);

            return events;
        }

        private async Task<List<DomainEvent>> GetAndDispatchEventsAsync()
        {
            // Our unit of work tracks entities changed during command handling, like Entity Framework does
            //  (except that ours is currently not so smart and it relies on command handlers calling repositories' Store method
            //  to track modified entities)
            // So to get and dispatch the events we have to:

            // Get the tracked entities and from them the events raised:
            var eventTrackingEntities = _unitOfWork.GetTrackedEntities().
                            Where(i => i is IDomainEventTracker).
                            Cast<IDomainEventTracker>();
            var newEvents = eventTrackingEntities.SelectMany(i => i.Events).ToList();

            // Clear up the events from the entities, 
            //  in case the event handlers about to be executed raise new events:
            eventTrackingEntities.ToList().ForEach(i => i.ClearEvents());

            // Then dispatch the events:
            var tasks = newEvents.Select(async (newEvent) =>
                {
                    newEvent.TriggeredBy = _unitOfWork.CurrentUser;
                    await _mediator.Publish(newEvent);
                });

            await Task.WhenAll(tasks);

            return newEvents;
        }
    }
}
