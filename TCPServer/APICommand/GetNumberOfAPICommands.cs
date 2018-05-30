using System;
using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class GetNumberOfAPICommands : CommandBase
    {
        public override string Description
        {
            get
            {
                return "Obtain number of available API Commands";
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
                int num = Server.GetNumberOfAPICommands();
                string feedBackMessage = string.Format("Number of available API commands: {0}", num);

                return new ValidFeedBack(feedBackMessage);
            }

            return InvalidInputParameter();
        }
    }
}
