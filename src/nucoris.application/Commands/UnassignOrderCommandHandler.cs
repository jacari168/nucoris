using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class UnassignOrderCommandHandler
        : IRequestHandler<UnassignOrderCommand, CommandResult>
    {
        private readonly ICommandSession _commandSession;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public UnassignOrderCommandHandler(
            ICommandSession commandSession,
            IOrderRepository orderRepository,
            IUserRepository userRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<CommandResult> Handle(UnassignOrderCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");
            Guard.Against.Condition(cmd.OrderId == default(Guid), "Invalid order id");
            Guard.Against.Condition(cmd.UserId == default(Guid), "Invalid user id");

            domain.User user = await _userRepository.GetAsync(cmd.UserId);
            if (user == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound, $"User {cmd.UserId} not found.");
            }

            var order = await _orderRepository.GetAsync(cmd.PatientId, cmd.OrderId);
            order.Unassign();

            _orderRepository.Store(order);

            var success = await _commandSession.CommitAsync();

            return new CommandResult(success);
        }
    }


}
