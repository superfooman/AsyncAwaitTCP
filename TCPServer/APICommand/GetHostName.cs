using System;
using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class GetHostName : CommandBase
    {
        public override string Description
        {
            get
            {
                return "Obtain host machine name";
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
                string hostName = Server.GetHostName();
                string feedBackMessage = string.Format("Host name is {0}:", hostName);

                return new ValidFeedBack(feedBackMessage);
            }

            return InvalidInputParameter();
        }
    }
}
