using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace VTT
{
    public class ImageTile : Image
    {
        private static int img_id = 0;

        public static int GetIDValue()
        {
            return img_id;
        }

        public static int AssignNextID()
        {
            return ++img_id;
        }

        public static void ResetID()
        {
            img_id = 0;
        }

        public static void SetID(int value)
        {
            img_id = value;
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
    }
    [Serializable()]
    public class CharacterSheet
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int ArmorClass { get; set; }
        public int Initiative { get; set; }
        public string LayerMode { get; set; }

        public CharacterSheet()
        {
            Name = string.Empty;
            HP = 0;
            ArmorClass = 0;
            Initiative = 0;
        }
    }

    [DataContract, Serializable()]
    public class TileToTransfer
    {
        [DataMember]
        public byte[] Source { get; set; }
        [DataMember]
        public string LayerMode { get; set; }
        [DataMember]
        public Point PutPosition { get; set; }
        [DataMember]
        public CharacterSheet CharSheet { get; set; } //null == it's image tile
        [DataMember]
        public double Width { get; set; }
        [DataMember]
        public double Height { get; set; }
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
}
