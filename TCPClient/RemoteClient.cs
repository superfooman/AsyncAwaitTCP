using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCPClient
{
    public class RemoteClient
    {
        private readonly TcpClient remoteClient;
        private Task receivingMessageTask;

        #region Events
        public EventHandler<ClientEventArgs> ClientConnected;
        public EventHandler<ClientEventArgs> ClientDisconnected;
        public EventHandler<ClientDataReadEventArgs> ClientMessageDisplayed;
        public EventHandler<GeneralErrorEventArgs> ErrorHappened;
        #endregion

        #region Constructor
        public RemoteClient()
        {
            remoteClient = new TcpClient();
            receivingMessageTask = null;
        }
        #endregion

        #region public API
        public async Task Connect(IPAddress server, int port)
        {
            await remoteClient.ConnectAsync(server, port);
            receivingMessageTask = Task.Run(() => receiveMessageAysnc(remoteClient));
            OnClientConnected(remoteClient);
            await Task.WhenAll(receivingMessageTask);
        }

        public void Disconnect()
        {
            remoteClient.GetStream().Close();
            remoteClient.Close();
            OnClientDisconnected(remoteClient);
        }

        public void SendMessage(string message)
        {
            var writer = new StreamWriter(remoteClient.GetStream());
            writer.AutoFlush = true;
            try
            {
                Task.Factory.StartNew(async () => await writer.WriteLineAsync(message));
                //await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                writer.Close();
                OnErrorHappened(ex);
                OnClientDisconnected(remoteClient);
            }
        }
        #endregion

        #region private methods
        private async Task receiveMessageAysnc(TcpClient remoteClient)
        {
            NetworkStream networkStream = initNetworkStream(remoteClient);
            StreamReader reader = new StreamReader(networkStream);
            try
            {
                while (true)
                {
                    string message = await reader.ReadLineAsync();
                    if (!isConnectionAvailable(remoteClient))
                    {
                        throw new Exception("This remote client is now disconnected");
                    }
                    OnClientMessageDisplayed(message);

                }
            }
            catch (Exception ex)
            {
                OnErrorHappened(ex);
                OnClientDisconnected(remoteClient);
            }
            finally
            {
                reader.Close();
                networkStream.Close();
            }
        }
        
        private NetworkStream initNetworkStream(TcpClient client)
        {
            // clear(read) any unwanted network stream input buffer during initilization
            // if there is no unwanted network stream input, skip the read/clear buffer action
            NetworkStream networkStream = client.GetStream();

            if (networkStream.DataAvailable)
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];
                networkStream.Read(buffer, 0, buffer.Length);
            }

            return networkStream;
        }

        private bool isConnectionAvailable(TcpClient remoteClient)
        {
            // check if the remote connection is responsive to the server (listener)
            try
            {
                bool status = true;
                if (remoteClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (remoteClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        return false;
                    }
                }
                return status;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Fire up the events (EventHandler)

        protected virtual void OnClientConnected(TcpClient client)
        {
            if (ClientConnected != null)
                ClientConnected(this, new ClientEventArgs(client));
        }

        protected virtual void OnClientDisconnected(TcpClient client)
        {
            if (ClientDisconnected != null)
                ClientDisconnected(this, new ClientEventArgs(client));
        }

        protected virtual void OnClientMessageDisplayed(string message)
        {
            if (ClientMessageDisplayed != null)
                ClientMessageDisplayed(this, new ClientDataReadEventArgs(message));
        }

        protected virtual void OnErrorHappened(Exception error)
        {
            if (ErrorHappened != null)
                ErrorHappened(this, new GeneralErrorEventArgs(error));
        }
        #endregion

    }
}

