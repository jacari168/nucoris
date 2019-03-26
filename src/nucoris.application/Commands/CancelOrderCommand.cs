using System;

namespace nucoris.application.commands
{
    public class CancelOrderCommand : MediatR.IRequest<CommandResult>
    {
        public Guid PatientId { get; }
        public Guid OrderId { get; }
        public string CancellationReason { get; }
        public Guid UserId { get; }

        public CancelOrderCommand(Guid patientId, Guid orderId,
            string cancellationReason, Guid userId)
        {
            PatientId = patientId;
            OrderId = orderId;
            CancellationReason = cancellationReason;
            UserId = userId;
        }
    }
}
