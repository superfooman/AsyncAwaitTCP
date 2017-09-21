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
        private List<TcpClient> clients;
        private List<Task> ReceiveMessageTasks;

        public EventHandler<ClientConnectedEventArgs> ClientConnected;
        public EventHandler<ClientDataReadEventArgs> ClientDisconnected;
        public EventHandler<NumberOfClientConnectedEventArgs> NumberOfClientConnected;
        public EventHandler<ClientDataReadEventArgs> ClientDataRead;
        public EventHandler<GeneralErrorEventArgs> ErrorHappened;

        public TcpServer(IPAddress ipAddress, int port)
        {
            listener = new TcpListener(ipAddress, port);
            listen = false;

            clients = new List<TcpClient>();
            ReceiveMessageTasks = new List<Task>();
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

        private bool isConnectionAvaiable(TcpClient client)
        {
            try
            {
                bool status = true;
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        return false;
                    }
                }
                return status;
            }
            catch (Exception ex)
            {
                OnErrorHappened(ex);
                return false;
            }
        }
  
        private async Task acceptClientsAsync(TcpListener listener)
        {
            try
            {
                while (listen)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    ReceiveMessageTasks.Add(receiveMessagesAsync(client));
                    clients.Add(client);
                    OnClientConnected(client);
                    OnNumberOfConnectedClients(clients);
                }
                await Task.WhenAll(ReceiveMessageTasks);
            }
            catch (Exception ex)
            {
                OnErrorHappened(ex); 
            }
        }

        private async Task receiveMessagesAsync(TcpClient client)
        {
            NetworkStream netStream = client.GetStream();
            StreamReader reader = new StreamReader(netStream);
            try
            {
                while (listen)
                {
                    string message = await reader.ReadLineAsync();
                    if (!isConnectionAvaiable(client))  // message == null
                    {
                        throw new Exception("This client is now disconnected");
                    }
                    OnClientDataRead(client, message);
                }
            }
            catch (Exception ex)
            {
                OnDisconnected(client, ex.Message);
                clients.Remove(client);
                OnNumberOfConnectedClients(clients);
            }
            finally
            {
                netStream.Close();
                reader.Close();
            }
        }

        protected virtual void OnDisconnected(TcpClient client, string errorMessage)
        {
            if (ClientDisconnected != null)
                ClientDisconnected(this, new ClientDataReadEventArgs(client, errorMessage));
        }

        protected virtual void OnClientConnected(TcpClient client)
        {
            if (ClientConnected != null)
                ClientConnected(this, new ClientConnectedEventArgs(client) );
        }

        protected virtual void OnNumberOfConnectedClients(List<TcpClient> clients)
        {
            if (NumberOfClientConnected != null)
                NumberOfClientConnected(this, new NumberOfClientConnectedEventArgs(clients));
        }

        protected virtual void OnClientDataRead(TcpClient client, string message)
        {
            if (ClientDataRead != null)
                ClientDataRead(this, new ClientDataReadEventArgs(client, message));
        }

        protected virtual void OnErrorHappened(Exception error)
        {
            if (ErrorHappened != null)
                ErrorHappened(this, new GeneralErrorEventArgs(error));
        }
    }
}
