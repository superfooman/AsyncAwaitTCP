using System;
using System.Collections.Generic;
using System.Net.Sockets;
namespace TCPServer
{
    public class ClientDataReadEventArgs : EventArgs
    {
        public string RemoteEndPoint { get; set; }
        public string Message { get; set; }

        public ClientDataReadEventArgs(TcpClient client, string message)
        {
            RemoteEndPoint = client.Client.RemoteEndPoint.ToString();
            Message = message;
        }
    }
}