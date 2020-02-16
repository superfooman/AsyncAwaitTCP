using System;
using System.Collections.Generic;
using System.Net.Sockets;
namespace TCPClient
{
    public class ClientDataReadEventArgs : EventArgs
    {
        public string Message { get; set; }

        public ClientDataReadEventArgs(string message)
        {
            Message = message;
        }
    }
}