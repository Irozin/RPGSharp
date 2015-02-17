using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace VTT
{
    public class ImageTile : Image
    {
        private static int next_img_id = 0;

        public static int GetIDValue()
        {
            return next_img_id;
        }

        public static int AssignNextID()
        {
            return ++next_img_id;
        }

        //for tiles and tokens
        public enum LayerModeEnum
        {
            Background,
            Normal,
            Hidden
        }
        public string LayerMode { get; set; }
        public Point ClickPosition { get; set; }
        public Point PutPosition { get; set; }
        public int ID { get; set; }
    }

    public class TokenTile : ImageTile
    {
        public CharacterSheet CharSheet { get; set; }

        public TokenTile()
        {
            CharSheet = new CharacterSheet();
        }
    }

    [DataContract]
    public class TileToTransfer
    {
        [DataMember]
        public byte[] Source { get; set; }
        [DataMember]
        public string LayerMode { get; set; }
        [DataMember]
        public Point PutPosition { get; set; }
        [DataMember]
        public bool CharSheet { get; set; } // false == imagetile, true == tokentile
        [DataMember]
        public double Width { get; set; }
        [DataMember]
        public double Height { get; set; }
        [DataMember]
        public Thickness Margin { get; set; }
        [DataMember]
        public int ID { get; set; }

        public static System.IO.MemoryStream SerializeImg(BitmapImage bimg)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bimg));
            encoder.Save(ms);
            return ms;
        }
        public BitmapImage DeserializeImg()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(Source);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.DecodePixelHeight = (int)Height;
            bi.DecodePixelWidth = (int)Width;
            bi.EndInit();
            return bi;
        }
    }

    [DataContract]
    public class TileTransferCollection
    {
        /*TODO: change Tiles every time a tile is changed*/
        [DataMember]
         public List<TileToTransfer> Tiles { get; set; }

        public TileTransferCollection()
        {
            Tiles = new List<TileToTransfer>();
        }
    }
}
