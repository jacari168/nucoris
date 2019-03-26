using System;
using Ardalis.GuardClauses;
using nucoris.domain;
using nucoris.application.interfaces;
using nucoris.queries.ActiveOrdersView;
using System.Threading;
using System.Threading.Tasks;
using nucoris.application.interfaces.repositories;

namespace nucoris.application.eventHandlers
{
    /// <summary>
    /// This class handles asynchronously events that affect active order change,
    /// with the goal to update the materialized view repository that feeds the active orders worklist
    /// </summary>
    public class ActiveOrderChangesEventHandler :
        MediatR.INotificationHandler<DeferredDomainEvent<OrderEvent>>,
        MediatR.INotificationHandler<DeferredDomainEvent<MedicationPrescribedEvent>>,
        MediatR.INotificationHandler<DeferredDomainEvent<OrderAssignedEvent>>,
        MediatR.INotificationHandler<DeferredDomainEvent<OrderCancelledEvent>>,
        MediatR.INotificationHandler<DeferredDomainEvent<OrderCompletedEvent>>,
        MediatR.INotificationHandler<DeferredDomainEvent<OrderStartedEvent>>,
        MediatR.INotificationHandler<DeferredDomainEvent<OrderUnassignedEvent>>
    {
        private readonly IMaterializedViewRepository<ActiveOrdersViewItem, ActiveOrdersViewSpecification> _queryRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ICommandSession _commandSession;

        public ActiveOrderChangesEventHandler(
            IMaterializedViewRepository<ActiveOrdersViewItem, ActiveOrdersViewSpecification> queryRepository,
            IPatientRepository patientRepository,
            ICommandSession commandSession)
        {
            _queryRepository = queryRepository;
            _patientRepository = patientRepository;
            _commandSession = commandSession;
        }

        public async Task Handle(DeferredDomainEvent<OrderEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        public async Task Handle(DeferredDomainEvent<MedicationPrescribedEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        public async Task Handle(DeferredDomainEvent<OrderAssignedEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        public async Task Handle(DeferredDomainEvent<OrderCancelledEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        public async Task Handle(DeferredDomainEvent<OrderCompletedEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        public async Task Handle(DeferredDomainEvent<OrderStartedEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        public async Task Handle(DeferredDomainEvent<OrderUnassignedEvent> notification, CancellationToken cancellationToken)
        {
            await HandleOrderEvent(notification.Event);
        }

        private async Task HandleOrderEvent(OrderEvent orderEvent)
        {
            var patient = await _patientRepository.GetAsync(orderEvent.PatientIdentity.Id);
            if (patient == null)
            {
                throw new InvalidOperationException($"Cannot find patient {orderEvent.PatientIdentity.Id} " +
                                                    $"when processing order event {orderEvent.Id}");
            }

            var queryItem = await _queryRepository.GetAsync(orderEvent.Order.Id);
            if (queryItem == null)
            {
                queryItem = ActiveOrdersViewItemFactory.FromDomain(patient, orderEvent.Order);
            }
            else
            {
                queryItem = ActiveOrdersViewItemFactory.FromExisting(queryItem, orderEvent.Order);
            }

            if (queryItem.QueryOrder.OrderState == nucoris.queries.ActiveOrdersView.OrderState.Inactive)
            {
                _queryRepository.Remove(queryItem);
            }
            else
            {
                _queryRepository.Store(queryItem);
            }

            var success = await _commandSession.CommitAsync();
        }
    }
}
