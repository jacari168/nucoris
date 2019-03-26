using System;

namespace nucoris.application.commands
{
    // Some commands end up created an entity.
    // If so, it's handy sometimes to return the Id of the created entity
    // (at least for testing)
    public class CommandResultWithCreatedEntityId : CommandResult
    {
        public Guid CreatedEntityId { get; }

        public CommandResultWithCreatedEntityId(Guid createdEntityId, bool isSuccessful)
            : base(isSuccessful)
        {
            this.CreatedEntityId = createdEntityId;
        }

        public CommandResultWithCreatedEntityId(Guid createdEntityId, CommandResultCode resultCode, string message)
            : base(resultCode, message)
        {
            this.CreatedEntityId = createdEntityId;
        }
    }
}