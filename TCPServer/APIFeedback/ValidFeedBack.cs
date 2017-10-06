namespace TCPServer.APIFeedback
{
    internal class ValidFeedBack : IFeedBack
    {
        public string FeedBack { get; private set; }

        public ValidFeedBack(string feedBack)
        {
            FeedBack = feedBack;
        }
    }
}
