using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class ServerForm: Form
    {
        private TcpServer server;
        private string host;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            ipaddressTextBox.Text = "127.0.0.1";
            portTextBox.Text = "6300";
            displayTextBox.ReadOnly = true;
            displayTextBox.ForeColor = Color.DarkBlue;
            statusBarUpdate("Idle", Color.Yellow);
            host = "LocalHost";
            AcceptButton = sendButton;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress ipaddress;
                int port;

                if (validate(out ipaddress, out port))
                {
                    statusBarUpdate("Waiting", Color.Aqua);
                    server = new TcpServer(ipaddress, port);
                    server.ClientConnected += server_ClientConnected;
                    server.ClientDisconnected += server_ClientDisconnected;
                    server.NumberOfClientConnected += server_NumberOfClientConnected;
                    server.ClientMessageDisplayed += server_ClientMessageDisplayed;
                    server.ErrorHappened += server_ErrorHappened;
                    server.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to close the TCP server", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (server != null)
                    server.Stop();
                this.Close();
            }           
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                if ((server.IsConnectingToClient) && !string.IsNullOrEmpty(senderTextBox.Text))
                {
                    displayMessage(host, senderTextBox.Text);
                    server.BroadCast(server.HostID, senderTextBox.Text);
                }
            }
            senderTextBox.Text = "";
        }

        private bool validate(out IPAddress ipaddress, out int port)
        {
            if (!int.TryParse(portTextBox.Text, out port))
            {
                port = 6300;
            }
            if (!IPAddress.TryParse(ipaddressTextBox.Text, out ipaddress))
            {
                MessageBox.Show("The IP address entered was invalid.");
                return false;
            }
            return true;
        }

        private void statusBarUpdate(string message, Color color)
        {
            toolStripStatusLabel.Text = String.Format("Status: {0}...", message);
            toolStripStatusLabel.BackColor = color;
        }

        private void displayMessage(string name, string message)
        {
            Action<string, string> action = (stringx, stringy) =>
            {
                displayTextBox.Text += string.Format("[{0}]: {1}", stringx, stringy);
                displayTextBox.SelectionStart = displayTextBox.Text.Length;
                displayTextBox.ScrollToCaret();
            };

            if (this.InvokeRequired)
            {
                this.Invoke(action, new object[] { name, message + Environment.NewLine });
            }
            else
            {
                action(name, message + Environment.NewLine);
            }
        }

        private void server_NumberOfClientConnected(object sender, NumberOfClientConnectedEventArgs args)
        {
            Action<string, Color> action = statusBarUpdate;

            if (this.InvokeRequired)
            {
                if (args.NumberOfClients > 0)
                    this.Invoke(action, new object[] { string.Format("Connection x {0}", args.NumberOfClients), Color.LightGreen });
                if (args.NumberOfClients == 0)
                    statusBarUpdate("Waiting", Color.Aqua);
            }
            else
            {
                if (args.NumberOfClients > 0)
                    action(string.Format("Connection x {0}", args.NumberOfClients), Color.LightGreen);
                if (args.NumberOfClients == 0)
                    statusBarUpdate("Waiting", Color.Aqua);
            }
        }

        private void server_ClientConnected(object sender, ClientConnectedEventArgs args)
        {
            displayMessage(args.RemoteEndPoint, "Successfully connected");
        }

        private void server_ClientDisconnected(object sender, ClientDataReadEventArgs args)
        {
            displayMessage(args.RemoteEndPoint, args.Message);
        }

        private void server_ClientMessageDisplayed(object sender, ClientDataReadEventArgs args)
        {
            displayMessage(args.RemoteEndPoint, args.Message);
        }

        private void server_ErrorHappened(object sender, GeneralErrorEventArgs args)
        {
            MessageBox.Show("Error: " + args.Error.Message, "Error");
        }
    }
}
