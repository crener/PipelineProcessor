using System.Collections.Generic;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;

namespace PipelineProcessor2.Nodes.Sample
{
    public class bluring : IProcessPlugin
    {
        public int InputQty => 1;
        public int OutputQty => 1;

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Image Blur";
            else if (request == PluginInformationRequests.Description) return "Applies blurring to an image";
            else if (request == PluginInformationRequests.InputName)
                if (index == 0) return "image";
            else if (request == PluginInformationRequests.InputType)
                if (index == 0) return "jpg";
            else if (request == PluginInformationRequests.OutputName)
                if (index == 0) return "image";
            else if (request == PluginInformationRequests.OutputType)
                if (index == 0) return "jpg";

            return "";
        }

        public byte[] ProcessData(List<byte[]> input)
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

                byte[] output = outStream.GetBuffer();
                return output;
            }
        }
    }
}
