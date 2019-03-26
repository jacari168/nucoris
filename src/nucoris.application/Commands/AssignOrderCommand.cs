using System;

namespace nucoris.application.commands
{
    public class AssignOrderCommand : MediatR.IRequest<CommandResult>
    {
        public Guid PatientId { get; }
        public Guid OrderId { get; }
        public Guid AssignedUserId { get; }
        public Guid UserId { get; }

        public AssignOrderCommand(Guid patientId, Guid orderId,
            Guid assignedUserId, Guid userId)
        {
            PatientId = patientId;
            OrderId = orderId;
            AssignedUserId = assignedUserId;
            UserId = userId;
        }
    }
}
