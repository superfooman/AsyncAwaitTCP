using System;
using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class GetNumberOfClients : CommandBase
    {
        public override string Description
        {
            get
            {
                return "Obtain number of connected clients";
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
                int num = Server.GetNumberOfClients();
                string feedBackMessage = string.Format("Number of connected clients: {0}", num);

                return new ValidFeedBack(feedBackMessage);
            }

            return InvalidInputParameter();
        }
    }
}
