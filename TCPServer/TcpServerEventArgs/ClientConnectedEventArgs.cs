using System;
using System.Net.Sockets;

namespace TCPServer
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public string RemoteEndPoint { get; set; }

        public ClientConnectedEventArgs(TcpClient client)
        {
            RemoteEndPoint = client.Client.RemoteEndPoint.ToString();
        }
    }
}