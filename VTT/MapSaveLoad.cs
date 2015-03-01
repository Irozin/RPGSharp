using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows;

namespace VTT
{
    public static class MapSaveLoad
    {
        public static void SaveMap(MapInfo mapInfo)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                Stream stream = null;
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    formatter.Serialize(stream, mapInfo);
                    stream.Close();
                    MessageBox.Show("Map saved successfully.");
                }
                catch
                {
                    MessageBox.Show("Unable to save map.");
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
                
            }
        }

        public static void LoadMap(MainWindow window)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                Stream stream = null;
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    MapInfo mapInfo = (MapInfo)formatter.Deserialize(stream);
                    stream.Close();
                    window.CreateMap(mapInfo.tileWidth, mapInfo.tileHeight, mapInfo.mapHeight, mapInfo.mapWidth);
                    window.ListOfTiles = mapInfo.gameMap;
                }        
                catch
                {
                    MessageBox.Show("Unable to load map.");
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
        }
    }
    [Serializable()]
    public class MapInfo
    {
        public List<TileToTransfer> gameMap;
        public int mapHeight;
        public int mapWidth;
        public int tileHeight;
        public int tileWidth;
    }
}
