using System.Collections.Generic;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;

namespace PipelineProcessor2.Nodes
{
    public class bluring : IProcessPlugin
    {
        public int InputQty => 1;
        public int OutputQty => 1;

        public string PluginInformation(pluginInformationRequests request, int index)
        {
            if (request == pluginInformationRequests.Name) return "Image Blur";
            else if (request == pluginInformationRequests.description) return "Applies blurring to an image";
            else if (request == pluginInformationRequests.inputName)
            {
                if (index == 0) return "image";
            }
            else if (request == pluginInformationRequests.inputType)
            {
                if (index == 0) return "jpg";
            }
            else if (request == pluginInformationRequests.outputName)
            {
                if (index == 0) return "image";
            }
            else if (request == pluginInformationRequests.outputType)
            {
                if (index == 0) return "jpg";
            }

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
