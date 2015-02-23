using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VTT
{
    /// <summary>
    /// Interaction logic for JoinSettings.xaml
    /// </summary>
    public partial class JoinSettings : Window
    {
        public bool _Join { get; private set; }
        public string _Name { get; private set; }
        public string _IP { get; private set; }
        public string _Port { get; private set; }
        public JoinSettings()
        {
            InitializeComponent();
            _Join = false;
            _Name = string.Empty;
            _IP = string.Empty;
            _Port = string.Empty;
        }

        private void joinButton_Click(object sender, RoutedEventArgs e)
        {
            if (ipBox.Text == string.Empty || nameBox.Text == string.Empty)
            {
                MessageBox.Show("Please fill in all fields.");
            }
            else
            {
                _Join = true;
                _Name = nameBox.Text;
                _IP = ipBox.Text;
                _Port = portBox.Text;
                this.Close();
            } 
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}