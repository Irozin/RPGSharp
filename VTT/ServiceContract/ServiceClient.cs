using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace VTT
{
    public class ServiceClient : IServiceContractCallback
    {
        public string Name { get; private set; }
        private RichTextBox rtb;
        public PlayerType PT { get; private set; }
        
        //for map
        private int TILE_WIDTH;
        private int TILE_HEIGHT;
        private int MAP_HEIGHT;
        private int MAP_WIDTH;
        private MainWindow window;

        public ServiceClient(string name, RichTextBox rtb, PlayerType pt, MainWindow window)
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
        
        public void HostSetTileMapSizes(int TILE_HEIGHT, int TILE_WIDTH, int MAP_HEIGHT, int MAP_WIDTH)
        {
            this.TILE_HEIGHT = TILE_HEIGHT;
            this.TILE_WIDTH = TILE_WIDTH;
            this.MAP_HEIGHT = MAP_HEIGHT;
            this.MAP_WIDTH = MAP_WIDTH;
        }

        #region IServiceContractCallback members
        public void ReceiveMessage(string message, string userName)
        {
            rtb.AppendText(userName + ": " + message + Environment.NewLine);
        }

        public void ReceiveMap(List<TileToTransfer> map, int mapH, int mapW, int tileH, int tileW)
        {
            TILE_HEIGHT = tileH;
            TILE_WIDTH = tileW;
            MAP_HEIGHT = mapH;
            MAP_WIDTH = mapW;
            window.CreateMap(tileW, tileH, mapH, mapW);
            window.SetListOfTiles(map);
            foreach (var t in map)
            {
                ClientTileAdded(t);
            }
        }

        public void ClientTileMoved(int ID, Point newPos)
        {
            foreach (var t in window.map.Children)
            {
                if (t is TokenTile)
                {
                    var temp = t as TokenTile;
                    if (temp.ID == ID)
                    {
                        temp.Margin = new Thickness(newPos.X, newPos.Y, 0, 0);
                        temp.PutPosition = newPos;
                        break;
                    }
                }
            }
        }

        public void ClientTileAdded(TileToTransfer tile)
        {
            ImageTile tileToAdd;
            if (tile.CharSheet != null)
            {
                tileToAdd = new TokenTile();
                var temp = tileToAdd as TokenTile;
                temp.CharSheet = tile.CharSheet;
            }
            else
            {
                tileToAdd = new ImageTile();
            }
            tileToAdd.PutPosition = tile.PutPosition;
            tileToAdd.Margin = new Thickness(tileToAdd.PutPosition.X, tileToAdd.PutPosition.Y, 0, 0);
            tileToAdd.Height = TILE_HEIGHT;
            tileToAdd.Width = TILE_WIDTH;
            tileToAdd.Source = tile.DeserializeImg(TILE_HEIGHT, TILE_WIDTH);
            tileToAdd.LayerMode = tile.LayerMode;
            tileToAdd.ID = tile.ID;

            //check if tile is hidden- if so then hide it from player
            if (tileToAdd.LayerMode == ImageTile.LayerModeEnum.Hidden.ToString() &&
                this.PT == PlayerType.Player)
            {
                tileToAdd.Visibility = Visibility.Hidden;
            }
            window.map.Children.Add(tileToAdd);
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

        public void ClientCharSheetChanged(int ID, CharacterSheet cs)
        {
            foreach (var t in window.map.Children)
            {
                if (t is TokenTile)
                {
                    var temp = t as TokenTile;
                    if (temp.ID == ID)
                    {
                        temp.CharSheet = cs;
                        temp.LayerMode = cs.LayerMode;
                        switch ((ImageTile.LayerModeEnum)Enum.Parse(typeof(ImageTile.LayerModeEnum), temp.LayerMode))
                        {
                            case ImageTile.LayerModeEnum.Background: goto case ImageTile.LayerModeEnum.Normal;
                            case ImageTile.LayerModeEnum.Normal:
                                {
                                    temp.Visibility = Visibility.Visible;
                                    break;
                                }
                            case ImageTile.LayerModeEnum.Hidden:
                                {
                                    if (this.PT == PlayerType.Player)
                                    {
                                        temp.Visibility = Visibility.Hidden;
                                    }
                                    else
                                    {
                                        temp.Visibility = Visibility.Visible;
                                    }
                                    break;
                                }  
                            default:
                                {
                                    temp.Visibility = Visibility.Visible;
                                    break;
                                }
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Download map from server and save it to file
        /// </summary>
        /// <param name="map"></param>
        public void ReceiveMapToSave(List<TileToTransfer> map)
        {
            window.SetListOfTiles(map);
            MainWindow.SaveMap(map, TILE_HEIGHT, TILE_WIDTH, MAP_HEIGHT, MAP_WIDTH);
        }
        #endregion
    }
}
