using Ardalis.GuardClauses;
using MediatR;
using nucoris.application.interfaces;
using nucoris.application.interfaces.repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.commands
{
    public class AssignOrderCommandHandler
        : IRequestHandler<AssignOrderCommand, CommandResult>
    {
        private readonly ICommandSession _commandSession;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public AssignOrderCommandHandler(
            ICommandSession commandSession,
            IOrderRepository orderRepository,
            IUserRepository userRepository)
        {
            _commandSession = commandSession ?? throw new ArgumentNullException(nameof(commandSession));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<CommandResult> Handle(AssignOrderCommand cmd, CancellationToken cancellationToken)
        {
            Guard.Against.Condition(cmd.PatientId == default(Guid), "Invalid patient id");
            Guard.Against.Condition(cmd.OrderId == default(Guid), "Invalid order id");
            Guard.Against.Condition(cmd.AssignedUserId == default(Guid), "Invalid assigned user id");
            Guard.Against.Condition(cmd.UserId == default(Guid), "Invalid user id");

            domain.User assignedUser = await _userRepository.GetAsync(cmd.AssignedUserId);
            if (assignedUser == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound, $"User {cmd.AssignedUserId} not found.");
            }

            domain.User user = await _userRepository.GetAsync(cmd.UserId);
            if (user == null)
            {
                return new CommandResult(CommandResultCode.EntityNotFound, $"User {cmd.UserId} not found.");
            }

            var order = await _orderRepository.GetAsync(cmd.PatientId, cmd.OrderId);
            order.AssignTo(assignedUser);

            _orderRepository.Store(order);

            var success = await _commandSession.CommitAsync();

            return new CommandResult(success);
        }
    }


}
