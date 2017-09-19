using System;
using System.Net.Sockets;

namespace TCPServer
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public string RemoteEndPoint { get; set; }
        public string Message { get; set; }

        public ClientConnectedEventArgs(TcpClient client, String message)
        {
            RemoteEndPoint = client.Client.RemoteEndPoint.ToString();
            Message = message;
        }
    }
}