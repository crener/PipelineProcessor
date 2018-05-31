using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using PluginTypes;

namespace ImageNodes
{
    public class ImageOverlayText : IProcessPlugin
    {
        #region Node settings
        public int InputQty => 2;
        public int OutputQty => 1;
        public string Name => "Image Text Overlay";
        public string Description => "Overlays text over an image";

        public string OutputType(int slot)
        {
            if (slot == 0) return "jpg";
            return "";
        }

        public string OutputName(int slot)
        {
            if (slot == 0) return "Image";
            return "";
        }

        public string InputType(int slot)
        {
            if (slot == 0) return "jpg";
            if (slot == 1) return "string";
            return "";
        }

        public string InputName(int slot)
        {
            if (slot == 0) return "image";
            if (slot == 1) return "text";
            return "";
        }
        #endregion

        public List<byte[]> ProcessData(List<byte[]> input)
        {
            if (input.Count < InputQty) return null;

            string text = Encoding.ASCII.GetString(input[1]);

            using (MemoryStream inStream = new MemoryStream(input[0]))
            using (MemoryStream outStream = new MemoryStream())
            {
                using (ImageFactory imageFactory = new ImageFactory())
                {
                    TextLayer textLayer = new TextLayer { Text = text };
                    imageFactory
                        .Load(inStream)
                        .Watermark(textLayer)
                        .Quality(100)
                        .Format(new JpegFormat())
                        .Save(outStream);
                }

                List<byte[]> output = new List<byte[]>();
                output.Add(outStream.ToArray());

                return output;
            }
        }
    }
}
