using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

// Wrapper around the TCPListener class
namespace TCPServer
{
    public class TcpServer
    {
        private readonly TcpListener listener;
        private bool listen;
        private Task acceptClientTask;
        private List<TcpClient> clients = new List<TcpClient>();

        public EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
        public EventHandler<NumberOfClientConnectedEventArgs> NumberOfClientConnectedEvent;

        public TcpServer(IPAddress ipAddress, int port)
        {
            listener = new TcpListener(ipAddress, port);
            listen = false;
        }

        public void Start()
        {
            listener.Start();
            listen = true;

            acceptClientTask = Task.Run(() => acceptClientsAsync(listener));

            Task.WhenAll(acceptClientTask);
        }

        public void Stop()
        {
            listen = false;
        }

        public bool IsConnectionAvaiable()
        {
            return (clients.Count != 0);
        }

        private async Task acceptClientsAsync(TcpListener listener)
        {
            try
            {
                while (listen)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    OnClientConnected(client);
                    clients.Add(client);
                    OnNumberOfConnectedClients(clients);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected virtual void OnClientConnected(TcpClient client)
        {
            if (ClientConnectedEvent != null)
                ClientConnectedEvent(this, new ClientConnectedEventArgs(client, "Connected successfully") );
        }

        protected virtual void OnNumberOfConnectedClients(List<TcpClient> clients)
        {
            if (NumberOfClientConnectedEvent != null)
                NumberOfClientConnectedEvent(this, new NumberOfClientConnectedEventArgs(clients));
        }
    }
}
