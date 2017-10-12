using System;
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

        public override IFeedBack ProcessCommand(TcpServer Server)
        {
            string hostName = Server.GetHostName();
            string feedBackMessage = string.Format("Host name is {0}:", hostName);

            return new ValidFeedBack(feedBackMessage);
        }
    }
}
