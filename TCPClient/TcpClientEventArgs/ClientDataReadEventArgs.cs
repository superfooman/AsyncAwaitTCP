using System;
using System.Collections.Generic;
using System.Net.Sockets;
namespace TCPClient
{
    public class ClientDataReadEventArgs : EventArgs
    {
        public string RemoteEndPoint { get; set; }
        public string Message { get; set; }

        public ClientDataReadEventArgs(TcpClient remoteClient, string message)
        {
            RemoteEndPoint = remoteClient.Client.RemoteEndPoint.ToString();
            Message = message;
        }
    }
}