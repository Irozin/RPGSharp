using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace VTT
{
    public class ChatClient : IChatCallback
    {
        public string Name { get; private set; }
        private RichTextBox rtb;
        public PlayerType PT { get; private set; }

        //for map
        //readonly
        public int TILE_WIDTH;
        public int TILE_HEIGHT;
        public MainWindow window;

        public ChatClient(string name, RichTextBox rtb, PlayerType pt, MainWindow window)
        {
            Name = name;
            this.rtb = rtb;
            PT = pt;
            this.window = window;
        }
        public enum PlayerType
        {
            GameMaster,
            Player
        }

        #region IChatCallback members
        public void ReceiveMessage(string message, string userName)
        {
            rtb.AppendText(userName + ": " + message + Environment.NewLine);
        }

        public void ReceiveMap(List<TileToTransfer> map, int mapH, int mapW, int tileH, int tileW)
        {
            TILE_HEIGHT = tileH;
            TILE_WIDTH = tileW;
            window.CreateMap(tileH, tileW, mapH, mapW);
            //this.gameMap = map;
            foreach (var t in map)
            {
                ClientTileAdded(t);
            }
        }

        public void ClientTileMoved(TileToTransfer tile)
        {
            foreach (var t in window.map.Children)
            {
                if (t is ImageTile)
                {
                    var temp = t as ImageTile;
                    if (temp.ID == tile.ID)
                    {
                        temp.Margin = new Thickness(tile.PutPosition.X, tile.PutPosition.Y, 0, 0);
                        temp.PutPosition = tile.PutPosition;
                        break;
                    }
                }
            }
        }

        public void ClientTileAdded(TileToTransfer tile)
        {
            if (tile.CharSheet == false)
            {
                window.map.Children.Add(new ImageTile
                {
                    Source = tile.DeserializeImg(),
                    Margin = tile.Margin,
                    Height = tile.Height,
                    Width = tile.Width,
                    LayerMode = tile.LayerMode,
                    PutPosition = tile.PutPosition,
                    ID = tile.ID
                });
            }
            else
            {
                window.map.Children.Add(new TokenTile
                {
                    Source = tile.DeserializeImg(),
                    Margin = tile.Margin,
                    Height = tile.Height,
                    Width = tile.Width,
                    LayerMode = tile.LayerMode,
                    PutPosition = tile.PutPosition,
                    ID = tile.ID,
                    CharSheet = new CharacterSheet()
                });
            }
        }

        public void ClientTileDeleted(int ID)
        {
            foreach (var t in window.map.Children)
            {
                if (t is ImageTile)
                {
                    var temp = t as ImageTile;
                    if (temp.ID == ID)
                    {
                        window.map.Children.Remove(temp);
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
