using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;

namespace VTT
{
    public class GraphicsProvider
    {
        #region variables
        private readonly int TILE_WIDTH;
        private readonly int TILE_HEIGHT;
        #endregion

        public GraphicsProvider(int tileHeight, int tileWidth) 
        {
            TILE_HEIGHT = tileHeight;
            TILE_WIDTH = tileWidth;
        }
        /*By far it doesn't support image categories
         *(images stored in files like Tokens images
         *and Backrogund images)- all images
         *are stored in one list
         */
        public List<BitmapImage> LoadGraphics(string path)
        {
            //this list will be returned
            List<BitmapImage> imgList = new List<BitmapImage>();
            List<string> imgExtensions = new List<string> 
            {
                ".JPG", ".PNG", ".BMP", ".GIF", ".JPE"
            };
            var files = Directory.GetFiles(path);
            foreach (var img in files)
            {
                //check if file is image file by checking its extension
                if (imgExtensions.Contains(Path.GetExtension(img).ToUpper()))
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(img);
                    bi.DecodePixelHeight = TILE_HEIGHT;
                    bi.DecodePixelWidth = TILE_WIDTH;
                    bi.EndInit();
                    imgList.Add(bi);
                }
            }
            return imgList;
        }
    }
}
