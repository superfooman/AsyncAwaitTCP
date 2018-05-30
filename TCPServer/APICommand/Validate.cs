using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class Validate : CommandBase
    {
        public override string Description
        {
            get
            {
                return ("Check to make sure TCP server is active");
            }
        }

        public override IArgs[] Arguments
        {
            get { return new IArgs[0]; }
        }

        public override IFeedBack ProcessCommand(TcpServer Server, string[] args)
        {
            if (ArgumentsValidation(args))
            {
                bool status = Server.Validate();
                string feedbackMessage;

                if (status)
                    feedbackMessage = "Server is active";
                else
                    feedbackMessage = "Server is not active";

                return new ValidFeedBack(feedbackMessage);
            }

            return InvalidInputParameter();
        }
    }
}


