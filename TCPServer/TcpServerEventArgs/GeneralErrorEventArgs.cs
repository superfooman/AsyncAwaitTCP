using System;
namespace TCPServer
{
    public class GeneralErrorEventArgs : EventArgs
    {
        public Exception Error { get; set; }

        public GeneralErrorEventArgs(Exception error)
        {
            Error = error;
        }
    }
}