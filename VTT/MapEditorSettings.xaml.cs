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
    /// Interaction logic for MapEditorSettings.xaml
    /// </summary>
    public partial class MapEditorSettings : Window
    {
        public int _TileHeight { get; private set; }
        public int _TileWidth { get; private set; }
        public int _MapHeight { get; private set; }
        public int _MapWidth { get; private set; }
        public bool _CreateMap { get; private set; }

        public MapEditorSettings()
        {
            InitializeComponent();
            _CreateMap = false;
        }

        private void mapCreateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _TileHeight = int.Parse(tileHeight.Text);
                _TileWidth = int.Parse(tileWidth.Text);
                _MapHeight = int.Parse(mapHeight.Text);
                _MapWidth = int.Parse(mapWidth.Text);
                _CreateMap = true;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Error: couldn't make map with given values.");
            }
        }

        private void mapCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
