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

        public override IFeedBack ProcessCommand(TcpServer Server)
        {
            bool status = Server.Validate();
            string feedbackMessage;

            if (status)
                feedbackMessage = "Server is active";
            else
                feedbackMessage = "Server is not active";

            return new ValidFeedBack(feedbackMessage);            
        }
    }
}


