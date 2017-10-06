using System;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class GetNumberOfAPICommands : CommandBase
    {
        public override string Description
        {
            get
            {
                return "Descritipon: Obtain number of available API Commands";
            }
        }

        public override IFeedBack ProcessCommand(TcpServer Server)
        {
            int num = Server.GetNumberOfAPICommands();
            string feedBackMessage = string.Format("Number of available API commands: {0}", num);

            return new ValidFeedBack(feedBackMessage);
        }
    }
}
