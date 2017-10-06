using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    public interface ICommand
    {
        string Command { get; }
        string Description { get; }

        IFeedBack ProcessCommand(TcpServer Server);
    }
}
