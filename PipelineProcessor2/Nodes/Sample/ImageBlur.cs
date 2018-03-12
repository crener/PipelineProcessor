using System.Collections.Generic;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;

namespace PipelineProcessor2.Nodes.Sample
{
    public class ImageBlur : IProcessPlugin
    {
        #region Node settings
        public int InputQty => 1;
        public int OutputQty => 1;
        public string Name => "Image Blur";
        public string Description => "Applies blurring to an image";

        public string OutputType(int slot)
        {
            if (slot == 0) return "jpg";
            return "";
        }

        public string OutputName(int slot)
        {
            if (slot == 0) return "image";
            return "";
        }

        public string InputType(int slot)
        {
            if (slot == 0) return "jpg";
            return "";
        }

        public string InputName(int slot)
        {
            if (slot == 0) return "image";
            return "";
        }
        #endregion

        public List<byte[]> ProcessData(List<byte[]> input)
        {
            if (input.Count == 0) return null;

            using (MemoryStream inStream = new MemoryStream(input[0]))
            using (MemoryStream outStream = new MemoryStream())
            {
                using (ImageFactory imageFactory = new ImageFactory())
                {
                    imageFactory.Load(inStream)
                        .GaussianBlur(3)
                        .Format(new JpegFormat())
                        .Quality(100)
                        .Save(outStream);
                }

                List<byte[]> output = new List<byte[]>();
                output.Add(outStream.ToArray());

                return output;
            }
        }
    }
}
