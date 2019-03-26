using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class EndAdmissionCommandHandler
        : IRequestHandler<EndAdmissionCommand, CommandResult>
    {
        private readonly ICommandSession _commandSession;
        private readonly IAdmissionRepository _admissionRepository;
        private readonly IOrderRepository _orderRepository;

        public EndAdmissionCommandHandler(
            ICommandSession commandSession,
            IAdmissionRepository admissionRepository,
            IOrderRepository orderRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _admissionRepository = admissionRepository ?? throw new ArgumentNullException(nameof(admissionRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<CommandResult> Handle(EndAdmissionCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");
            Guard.Against.Condition(cmd.AdmissionId == default(Guid), "Invalid admission id");

            var admission = await _admissionRepository.GetAsync(cmd.PatientId, cmd.AdmissionId);

            if( admission == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound,
                    $"Admission {cmd.AdmissionId} for patient {cmd.PatientId} not found.");
            }

            admission.End(cmd.EndTime);
            _admissionRepository.Store(admission);

            // Cancel all its active prescription:
            var activeOrders = await _orderRepository.GetOfAdmissionAsync(
                cmd.PatientId, cmd.AdmissionId, domain.OrderStateExtensions.GetActiveStates());
            foreach(var order in activeOrders)
            {
                order.Cancel();
                _orderRepository.Store(order);
            }

            var success = await _commandSession.CommitAsync();

            return new CommandResult(success);
        }
    }


}
