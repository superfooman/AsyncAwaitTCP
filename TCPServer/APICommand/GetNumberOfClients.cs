using System;
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

        public override IFeedBack ProcessCommand(TcpServer Server)
        {
            int num = Server.GetNumberOfClients();
            string feedBackMessage = string.Format("Number of connected clients: {0}", num);

            return new ValidFeedBack(feedBackMessage);
        }
    }
}
