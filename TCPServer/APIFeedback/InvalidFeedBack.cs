namespace TCPServer.APIFeedback
{
    public enum InvalidFeedBackType
    {
        InvalidCommand,
        InvalidInputParameter
    }

    internal class InvalidFeedBack : IFeedBack
    {
        protected InvalidFeedBackType ErrorType { get; set; }

        public InvalidFeedBack(InvalidFeedBackType errorType)
        {
            ErrorType = errorType;
        }

        public string FeedBack
        {
            get
            {
                if (ErrorType == InvalidFeedBackType.InvalidCommand)
                    return "Invalid API command";
                else
                    return "Invalid Input Arugement";
            }
        }
    }
}
