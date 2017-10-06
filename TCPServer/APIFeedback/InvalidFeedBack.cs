namespace TCPServer.APIFeedback
{
    internal class InvalidFeedBack : IFeedBack
    {
        public string FeedBack
        {
            get
            {
                return "Invalid API command";
            }
        }
    }
}
