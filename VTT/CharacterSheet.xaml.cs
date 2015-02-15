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
    /// Interaction logic for CharacterSheet.xaml
    /// </summary>
    public partial class CharacterSheet : Window
    {
        public List<ChatClient> Owners { get; private set; } 
/*
 * TODO:
 * 1. przekazac wszystkich uzytkownikow do owners
 * 1a. update listy
 * 2. scrollowalna lista umiejętności z możliwością dodania nowych i usuwania
 */ 
        public CharacterSheet()
        {
            InitializeComponent();
            //currentClient = client;
            //if (currentClient.PT != ChatClient.PlayerType.Player)
            //{ 
            //    changeOwnerBtn.IsEnabled = false;
            //}
            abilitiesList.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }


        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
