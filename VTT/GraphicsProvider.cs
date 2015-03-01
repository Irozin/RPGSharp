using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace VTT
{
    public static class GraphicsProvider
    {
        /*By far it doesn't support image categories
         *(images stored in files like Tokens images
         *and Backrogund images)- all images
         *are stored in one list
         */
        public static List<BitmapImage> LoadGraphics(string path)
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
                    bi.DecodePixelHeight = 60;
                    bi.DecodePixelWidth = 60;
                    bi.EndInit();
                    imgList.Add(bi);
                }
            }
            return imgList;
        }
    }
}
