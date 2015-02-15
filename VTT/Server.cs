using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;

namespace VTT
{
    public class Server
    {
        private ServiceHost host;
        private DuplexChannelFactory<IChat> factory;
        private IChat channel;
        private static Server server;
        private MainWindow serverType;

        private Server(MainWindow serverType)
        {
            this.serverType = serverType;
        }

        public static Server GetInstance(MainWindow serverType)
        {
            if (server == null)
            {
                server = new Server(serverType);
            }
            return server;
        }

        public void HostGame(ChatClient chatClient, MainWindow window, System.Windows.Controls.RichTextBox rtb)
        {
            try
            {
                //host = new ServiceHost(typeof(MainWindow));
                host = new ServiceHost(serverType);
                host.Open();
                InstanceContext ic = new InstanceContext(chatClient);
                factory = new DuplexChannelFactory<IChat>(ic, "ChatClientPoint");
                channel = factory.CreateChannel();
                channel.Subscribe();
                MessageBox.Show("Server started successfuly");
                channel.SendMessage("*** " + chatClient.Name + " *** has joined", "Server");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void StopHosting()
        {
            if (host != null)
            {
                MessageBox.Show("Server stopped successfully");
                if (host.State != CommunicationState.Closed)
                {
                    factory.Close();
                    host.Close();
                    serverType.Close();
                }
            }
        }

        public void JoinGame(ChatClient chatClient, MainWindow window, System.Windows.Controls.RichTextBox rtb, 
            string userName, string address, string port)
        {
            try
            {
                InstanceContext ic = new InstanceContext(chatClient);
                Uri baseAddr = new Uri("net.tcp://" + address + ":" + port + "/VTT");
                factory = new DuplexChannelFactory<IChat>(ic, "ChatClientPoint", new EndpointAddress(baseAddr));
                channel = factory.CreateChannel();
                channel.SubscribePlayer();
                //channel.SendMap();
                channel.SendMessage("*** " + chatClient.Name + " *** has joined", "Server");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public bool IsHosting()
        {
            return host != null;
        }

        public IChat GetChannel()
        {
            return channel;
        }
    }
}
