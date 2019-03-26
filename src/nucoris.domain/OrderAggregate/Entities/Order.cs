using Ardalis.GuardClauses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace nucoris.domain
{
    public enum OrderState
    {
        Created = 1,
        Ready = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }

    public static class OrderStateExtensions
    {
        private static List<OrderState> _activeStates = 
            new List<OrderState>() { OrderState.Created, OrderState.InProgress, OrderState.Ready };

        public static IEnumerable<OrderState> GetActiveStates() => _activeStates.AsReadOnly();

        public static bool IsActive(this OrderState state)
        {
            return state == OrderState.Created
                || state == OrderState.InProgress
                || state == OrderState.Ready;
        }
    }

    public abstract class Order : AggregateRoot
    {
        public User OrderedBy { get; }
        public DateTime OrderedAt { get; }
        public Guid AdmissionId { get; }

        [JsonProperty]
        public string Description { get; protected set; }

        [JsonProperty]
        public User AssignedTo { get; private set; }

        [JsonProperty]
        public string CancellationReason { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] // to serialize as string
        public OrderState State { get; private set; }

        public Order(Guid id, PatientIdentity patientIdentity, Guid admissionId, User orderedBy, DateTime orderedAt) : base(id, patientIdentity)
        {
            Guard.Against.Null(orderedBy, "Ordering user");

            this.AdmissionId = admissionId;
            this.OrderedBy = orderedBy;
            this.OrderedAt = orderedAt;
            this.State = OrderState.Created;
        }

        public void AssignTo(User user)
        {
            Guard.Against.Condition( !this.State.IsActive(), $"Order {this.Id} is inactive");

            this.AssignedTo = user;
            this.State = OrderState.Ready;

            base.AddEvent(new OrderAssignedEvent(this, user));
        }

        public void Unassign()
        {
            Guard.Against.Condition(!this.State.IsActive(), $"Order {this.Id} is inactive");

            var previousAssignee = AssignedTo;
            this.AssignedTo = null;
            this.State = OrderState.Created;

            if (previousAssignee != null)
            {
                base.AddEvent(new OrderUnassignedEvent(this, previousAssignee));
            }
        }

        public void Start()
        {
            Guard.Against.InvalidState(State, OrderState.Completed, OrderState.Cancelled);

            this.State = OrderState.InProgress;
            base.AddEvent(new OrderStartedEvent(this));
        }

        public void Complete()
        {
            Guard.Against.InvalidState(State, OrderState.Cancelled);

            this.State = OrderState.Completed;
            base.AddEvent(new OrderCompletedEvent(this));
        }

        public void Cancel()
        {
            this.Cancel(cancellationReason: null);
        }

        public void Cancel(string cancellationReason)
        {
            Guard.Against.InvalidState(State, OrderState.Completed);

            this.AssignedTo = null;
            this.State = OrderState.Cancelled;
            this.CancellationReason = cancellationReason;

            base.AddEvent(new OrderCancelledEvent(this));
        }
    }
}