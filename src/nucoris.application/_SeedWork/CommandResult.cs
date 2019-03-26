namespace nucoris.application.commands
{
    public enum CommandResultCode
    {
        OK = 0,
        EntityNotFound = 1,
        InternalError = 2,
        BadRequest = 3
    }

    public class CommandResult
    {
        public bool Successful { get; }
        public CommandResultCode ResultCode { get; }
        public string Message { get; }

        public CommandResult(bool success)
        {
            ResultCode = (success ? CommandResultCode.OK : CommandResultCode.InternalError);
            Successful = success;
            Message = null;
        }

        public CommandResult(CommandResultCode resultCode, string message)
        {
            ResultCode = resultCode;
            Successful = (resultCode == CommandResultCode.OK);
            Message = message;
        }

    }
}