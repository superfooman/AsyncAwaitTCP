using System;
using System.Net.Sockets;

namespace TCPClient
{
    public class ClientEventArgs : EventArgs
    {
        public string RemoteEndPoint { get; set; }

        public ClientEventArgs(TcpClient remoteClient)
        {
            RemoteEndPoint = remoteClient.Client.RemoteEndPoint.ToString();
        }
    }
}