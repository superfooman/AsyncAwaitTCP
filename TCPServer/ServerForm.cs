using System;
using System.Data;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class ServerForm: Form
    {
        private TcpServer server;
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
                    server.ClientConnectedEvent += clientConnectedEventHandler;
                    server.NumberOfClientConnectedEvent += numberOfClientConnectedEventHandler;
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

        private void clientConnectedEventHandler(object sender, ClientConnectedEventArgs args)
        {
            Action<string, string> action = (stringx, stringy) =>
            {
                displayTextBox.Text += string.Format("[{0}]: {1}", stringx, stringy);
            };

            if (this.InvokeRequired)
            {
                this.Invoke(action, new object[] { args.RemoteEndPoint, args.Message + Environment.NewLine});
            }
            else
            {
                action(args.RemoteEndPoint, args.Message + Environment.NewLine);
            }
        }

        private void numberOfClientConnectedEventHandler(object sender, NumberOfClientConnectedEventArgs args)
        {
            Action<string, Color> action = statusBarUpdate;

            if (this.InvokeRequired)
            {
                if (args.NumberOfClients > 0)
                    this.Invoke(action, new object[] { string.Format("Connection x {0}", args.NumberOfClients), Color.LightGreen });
            }
            else
            {
                action(string.Format("Connection x {0}", args.NumberOfClients), Color.LightGreen);
            }
        }

    }
}
