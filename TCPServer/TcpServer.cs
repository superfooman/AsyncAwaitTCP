﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using TCPServer.APICommand;
using TCPServer.APIFeedback;

// Wrapper around the TCPListener class
namespace TCPServer
{
    public class TcpServer
    {
        private readonly TcpListener listener;
        private bool listen;
        private List<TcpClient> clients;
        private List<Task> receiveMessageTasks;
        private ICommand[] availableAPICommands;

        public string HostName
        {
            get { return Environment.MachineName; }
        }
        public string HostID
        {
            get { return listener.LocalEndpoint.ToString(); }
        }
        public bool IsListening
        {
            get { return listen; }
        }
        public bool IsConnectingToClient
        {
            get { return clients.Count > 0; }
        }
        public ICommand[] AvailableAPICommands
        {
            get
            {
                return availableAPICommands;
            }
        }

        public EventHandler<ClientConnectedEventArgs> ClientConnected;
        public EventHandler<ClientDataReadEventArgs> ClientDisconnected;
        public EventHandler<NumberOfClientConnectedEventArgs> NumberOfClientConnected;
        public EventHandler<ClientDataReadEventArgs> ClientMessageDisplayed;
        public EventHandler<GeneralErrorEventArgs> ErrorHappened;

        // Constructor
        public TcpServer(IPAddress ipAddress, int port)
        {
            listener = new TcpListener(ipAddress, port);
            listen = false;

            clients = new List<TcpClient>();
            receiveMessageTasks = new List<Task>();
            availableAPICommands = loadAvaialbeAPICommands();
        }

        private bool isConnectionAvailable(TcpClient client)
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

        private ICommand[] loadAvaialbeAPICommands()
        {
            Assembly serverAssembly = Assembly.GetAssembly(GetType());

            Type[] types = serverAssembly.GetTypes();

            List<ICommand> commands = new List<ICommand>();

            foreach (Type type in types)
            {
                if ((!type.IsAbstract) && (!type.IsInterface))
                {
                    if (typeof(ICommand).IsAssignableFrom(type))
                    {
                        commands.Add((ICommand)Activator.CreateInstance(type));
                    }
                }
            }
            return commands.ToArray();
        }

        private NetworkStream initNetworkStream(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            // clear(read) any unwanted network stream input buffer during initilization
            // if there is no unwanted network stream input, skip the read/clear buffer action
            if (networkStream.DataAvailable)
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];
                networkStream.Read(buffer, 0, buffer.Length);
            }

            return networkStream;
        }

        private void feedbackMessage(string hostId, string feedBack, TcpClient client)
        {
            if (listen)
            {
                Task task = Task.Run(async () =>
                {
                    var writer = new StreamWriter(client.GetStream());
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(string.Format("-----{0}-----", feedBack));
                });
            }
        }

        private async Task acceptClientsAsync(TcpListener listener)
        {
            try
            {
                while (listen)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    receiveMessageTasks.Add(receiveClientMessagesAsync(client));
                    clients.Add(client);
                    OnClientConnected(client);
                    OnNumberOfConnectedClients(clients);
                }
                await Task.WhenAll(receiveMessageTasks);
            }
            catch (Exception ex)
            {
                OnErrorHappened(ex); 
            }
        }

        private async Task receiveClientMessagesAsync(TcpClient client)
        {
            NetworkStream netStream = initNetworkStream(client);
            StreamReader reader = new StreamReader(netStream);
            try
            {
                while (listen)
                {
                    string message = await reader.ReadLineAsync();
                    if (!isConnectionAvailable(client))
                    {
                        throw new Exception("This client is now disconnected");
                    }
                    string[] args;
                    string clientAPICommand = CommandBase.GetAPICommand(message, out args);
                    if (clientAPICommand != null)
                    {
                        IFeedBack response;
                        ICommand APICommand = Array.Find(AvailableAPICommands, (item) => item.CommandName.ToLower() == clientAPICommand.ToLower());
                        if (APICommand != null)
                        {
                            response = APICommand.ProcessCommand(this, args);
                        }
                        else
                        {
                            response = CommandBase.InvalidCommand();
                        }
                        feedbackMessage(HostID, response.FeedBack, client);
                    }
                    else
                    {
                        OnClientMessageDisplayed(client, message);
                    }

                }
            }
            catch (Exception ex)
            {
                OnClientDisconnected(client, ex.Message);
                clients.Remove(client);
                OnNumberOfConnectedClients(clients);
            }
            finally
            {
                netStream.Close();
                reader.Close();
            }
        }

        protected virtual void OnClientDisconnected(TcpClient client, string errorMessage)
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

        protected virtual void OnClientMessageDisplayed(TcpClient client, string message)
        {
            if (ClientMessageDisplayed != null)
                ClientMessageDisplayed(this, new ClientDataReadEventArgs(client, message));
        }

        protected virtual void OnErrorHappened(Exception error)
        {
            if (ErrorHappened != null)
                ErrorHappened(this, new GeneralErrorEventArgs(error));
        }

        // Public API
        public async Task Start()
        {
            listener.Start();
            listen = true;

            await acceptClientsAsync(listener);

        }

        public void Stop()
        {
            listen = false;
            string disconnectMessage = "This client is now forced to disconnect";
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].GetStream().Close();
                clients[i].Close();
                OnClientDisconnected(clients[i], disconnectMessage);
            }
            clients.Clear();
            OnNumberOfConnectedClients(clients);

        }

        public void BroadCast(string hostId, string boradCastMessage)
        {
            if (listen)
            {
                Parallel.ForEach(clients, async (client) =>
                {
                    var writer = new StreamWriter(client.GetStream());
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(string.Format("[{0}]: {1}", hostId, boradCastMessage));
                });
            }
        }


        // ============================================ For demo purpose ======================================================== //
        // Simple client remote API commands
        internal bool Validate() 
        {
            return IsListening;
        }

        internal string GetHostName()
        {
            return HostName;
        }

        internal int GetNumberOfClients()
        {
            return clients.Count;
        }

        internal int GetNumberOfAPICommands()
        {
            return AvailableAPICommands.Length;
        }

        internal Dictionary<string, string> Help()
        {
            var commandDictionary = new Dictionary<string, string>();
            foreach (var APICommand in AvailableAPICommands)
            {
                commandDictionary.Add(APICommand.CommandName, APICommand.Description);
            }
            return commandDictionary;
        }

        // ============================================ For demo purpose ======================================================== //
        // Simple client remote API commands for Math operation (it can be another object or etc!)
        internal double Adder(double a, double b)
        {
            return a + b;
        }

        internal double Multiply(double a, double b)
        {
            return a * b;
        }

        internal int Sqrt(int num)
        {
            long lo = 0, hi = num;
            while (lo < hi)
            {
                long mid = lo + (hi - lo) / 2;
                if ((mid * mid <= num) && ((mid + 1) * (mid + 1) > num)) return (int)mid;
                if (mid * mid < num) lo = mid + 1;
                else hi = mid;
            }
            return (int)lo;
        }
    }
}
