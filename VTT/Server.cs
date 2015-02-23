using System;
using System.Windows;
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
                if (host.State != CommunicationState.Closed)
                {
                    factory.Close();
                    host.Close();
                    serverType.Close();
                    MessageBox.Show("Server stopped successfully");
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
                channel.SendMessage("*** " + chatClient.Name + " *** has joined", "Server");
            }
            catch
            {
                throw;
            }
        }

        public void PlayerCloseConnection()
        {
            if (factory != null)
            {
                factory.Close();
                MessageBox.Show("Disconnected from the server");
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
