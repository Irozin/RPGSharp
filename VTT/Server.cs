using System;
using System.Windows;
using System.ServiceModel;

namespace VTT
{
    public class Server
    {
        private ServiceHost host;
        private DuplexChannelFactory<ISerivceContract> factory;
        private ISerivceContract channel;
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

        public void HostGame(ServiceClient serviceClient, MainWindow window, System.Windows.Controls.RichTextBox rtb)
        {
            try
            {
                host = new ServiceHost(serverType);
                host.Open();
                InstanceContext ic = new InstanceContext(serviceClient);
                factory = new DuplexChannelFactory<ISerivceContract>(ic, "ClientServicePoint");
                channel = factory.CreateChannel();
                channel.Subscribe();
                MessageBox.Show("Server started successfuly");
                channel.SendMessage("*** " + serviceClient.Name + " *** has joined", "Server");
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

        public void JoinGame(ServiceClient serviceClient, MainWindow window, System.Windows.Controls.RichTextBox rtb, 
            string userName, string address, string port)
        {
            try
            {
                InstanceContext ic = new InstanceContext(serviceClient);
                Uri baseAddr = new Uri("net.tcp://" + address + ":" + port + "/VTT");
                factory = new DuplexChannelFactory<ISerivceContract>(ic, "ClientServicePoint", new EndpointAddress(baseAddr));
                channel = factory.CreateChannel();
                channel.SubscribePlayer();
                channel.SendMessage("*** " + serviceClient.Name + " *** has joined", "Server");
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

        public ISerivceContract GetChannel()
        {
            return channel;
        }
    }
}
