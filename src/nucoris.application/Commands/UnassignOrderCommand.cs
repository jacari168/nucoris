using System;

namespace nucoris.application.commands
{
    public class UnassignOrderCommand : MediatR.IRequest<CommandResult>
    {
        public Guid PatientId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }

        public UnassignOrderCommand(Guid patientId, Guid orderId, Guid userId)
        {
            PatientId = patientId;
            OrderId = orderId;
            UserId = userId;
        }
    }
}
