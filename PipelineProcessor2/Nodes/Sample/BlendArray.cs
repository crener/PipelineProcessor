using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;

namespace PipelineProcessor2.Nodes.Sample
{
    public class BlendArray : IProcessPlugin
    {
        public int InputQty => 1;
        public int OutputQty => 1;

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Image Blend";
            else if (request == PluginInformationRequests.Description) return "Blends all images in an array and overlays them";
            else if (request == PluginInformationRequests.InputName)
            {
                if (index == 0) return "Images";
            }
            else if (request == PluginInformationRequests.InputType)
            {
                if (index == 0) return "jpg[]";
            }
            else if (request == PluginInformationRequests.OutputName)
            {
                if (index == 0) return "Image";
            }
            else if (request == PluginInformationRequests.OutputType)
            {
                if (index == 0) return "jpg";
            }

            return "";
        }

        public List<byte[]> ProcessData(List<byte[]> input)
        {
            int imageQty = BitConverter.ToInt32(input[0], 0),
                opacity = 100 / imageQty;

            List<ImageLayer> layers = new List<ImageLayer>();
            Size largestSize = new Size(0, 0);
            for (var i = 1; i < input.Count; i++)
            {
                //load image into layer
                ImageLayer layer = new ImageLayer { Opacity = opacity };
                using (MemoryStream inStream = new MemoryStream(input[i]))
                    layer.Image = Image.FromStream(inStream);
                layer.Size = layer.Image.Size;

                if (layer.Image.Size.Width > largestSize.Width) largestSize.Width = layer.Image.Size.Width;
                if (layer.Image.Size.Height > largestSize.Height) largestSize.Height = layer.Image.Size.Height;

                layers.Add(layer);
            }

            ImageFactory image = new ImageFactory();
            image.Format(new JpegFormat());
            image.Quality(100);
            image.Load(new Bitmap(largestSize.Width, largestSize.Height));

            foreach(ImageLayer layer in layers) image.Overlay(layer);

            using (MemoryStream outStream = new MemoryStream())
            {
                image.Save(outStream);
                image.Dispose();

                List<byte[]> output = new List<byte[]>();
                output.Add(outStream.GetBuffer());
                return output;
            }
        }
    }
}
