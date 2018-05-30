using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    public interface ICommand
    {
        string CommandName { get; }
        string Description { get; }

        IArgs[] Arguments { get; }
        IFeedBack ProcessCommand(TcpServer Server, string[] args);
    }
}
