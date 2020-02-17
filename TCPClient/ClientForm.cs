using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPClient
{
    public partial class ClientForm : Form
    {
        private RemoteClient remoteClient;
        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            ipaddressTextBox.Text = "127.0.0.1";
            portTextBox.Text = "6300";
            displayTextBox.ReadOnly = true;
            displayTextBox.ForeColor = Color.DarkBlue;
            AcceptButton = sendButton;
        }

        private async void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress ipaddress;
                int port;

                if (validate(out ipaddress, out port))
                {
                    remoteClient = new RemoteClient();
                    remoteClient.ClientConnected += remoteClient_Connected;
                    remoteClient.ClientDisconnected += remoteClient_Disconnected;
                    remoteClient.ClientMessageDisplayed += remoteClient_MessageDisplayed;
                    remoteClient.ErrorHappened += remoteClient_ErrorHappened;
                    await remoteClient.Connect(ipaddress, port);
                }

                ipaddressTextBox.ReadOnly = true;
                portTextBox.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to close this remote client session", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (remoteClient != null)
                {
                    remoteClient.ClientConnected -= remoteClient_Connected;
                    remoteClient.ClientDisconnected -= remoteClient_Disconnected;
                    remoteClient.ClientMessageDisplayed -= remoteClient_MessageDisplayed;
                    remoteClient.ErrorHappened -= remoteClient_ErrorHappened;
                    remoteClient.Disconnect();
                }
                this.Close();
            }
        }
        private void sendButton_Click(object sender, EventArgs e)
        {
            if (remoteClient != null)
            {
                if (!string.IsNullOrEmpty(senderTextBox.Text))
                {
                    displayMessage(Environment.MachineName, senderTextBox.Text);
                    remoteClient.SendMessage(senderTextBox.Text);
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

        private void displayMessage(string message)
        {
            Action<string> action = (stringInput) =>
            {
                displayTextBox.Text += string.Format("{0}", stringInput);
                displayTextBox.SelectionStart = displayTextBox.Text.Length;
                displayTextBox.ScrollToCaret();
            };

            if (this.InvokeRequired)
            {
                this.Invoke(action, new object[] { message + Environment.NewLine });
            }
            else
            {
                action(message + Environment.NewLine);
            }
        }

        private void remoteClient_Connected(object sender, ClientEventArgs args)
        {
            Task.Factory.StartNew(() =>
            {
                displayMessage(args.RemoteEndPoint, "Successfully connected");
            });
        }

        private void remoteClient_Disconnected(object sender, ClientEventArgs args)
        {
            Task.Factory.StartNew(() =>
            {
                displayMessage(args.RemoteEndPoint, "Successfully disconnected");
            });
        }

        private void remoteClient_MessageDisplayed(object sender, ClientDataReadEventArgs args)
        {
            Task.Factory.StartNew(() =>
            {
                displayMessage(args.Message);
            });
        }

        private void remoteClient_ErrorHappened(object sender, GeneralErrorEventArgs args)
        {
            MessageBox.Show("Error: " + args.Error.Message, "Error");
        }
    }


}
