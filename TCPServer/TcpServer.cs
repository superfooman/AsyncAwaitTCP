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
        private Task acceptClientTask;
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
            // clear(read) any unwanted network stream input buffer during initilization
            NetworkStream networkStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            networkStream.Read(buffer, 0, buffer.Length);

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
                    await writer.WriteLineAsync(string.Format("[{0}]: {1}", hostId, feedBack));
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
                    if (!isConnectionAvaiable(client))
                    {
                        throw new Exception("This client is now disconnected");
                    }
                    OnClientMessageDisplayed(client, message);

                    string clientAPICommand = CommandBase.GetAPICommand(message);
                    if (clientAPICommand != null)
                    {
                        IFeedBack response;
                        ICommand APICommand = Array.Find(AvailableAPICommands, (item) => item.Command.ToLower() == clientAPICommand.ToLower());
                        if (APICommand != null)
                        {
                            response = APICommand.ProcessCommand(this);
                        }
                        else
                        {
                            response = CommandBase.InvalidCommand();
                        }
                        feedbackMessage(HostID, response.FeedBack, client);
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
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].GetStream().Close();
                clients[i].Close();
            }
            clients.Clear();
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
                commandDictionary.Add(APICommand.Command, APICommand.Description);
            }
            return commandDictionary;
        }
    }
}
