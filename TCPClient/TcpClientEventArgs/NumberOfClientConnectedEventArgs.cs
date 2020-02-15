using System;
using System.Collections.Generic;
using System.Net.Sockets;
namespace TCPServer
{
    public class NumberOfClientConnectedEventArgs : EventArgs
    {
        public int NumberOfClients { get; set; }

        public NumberOfClientConnectedEventArgs(List<TcpClient> clients)
        {
            NumberOfClients = clients.Count;
        }
    }
}