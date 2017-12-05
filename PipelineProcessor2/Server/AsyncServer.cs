using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Server.Exceptions;
using static System.String;

namespace PipelineProcessor2.Server
{
    public static class AsyncServer
    {
        static HttpListener listener = new HttpListener();

        private const int Port = 9980;
        private static Task listenThread;
        private static ResponseFactory responses = null;

        public static void StartListening()
        {
            if (listener.IsListening) listener.Stop();
            listener.Prefixes.Add("http://*:" + Port + "/");

            listener.Start();

            listenThread = new Task(Listen);
            listenThread.Start();

            if (responses == null) responses = new ResponseFactory();
        }

        private static void Listen()
        {
            while (listener.IsListening)
            {
                HttpListenerContext context = listener.GetContext();
                Console.WriteLine("Request received: " + context.Request.Url);

                context.Response.StatusCode = 200;
                try
                {
                    string result = responses.BuildResponse(context.Request);

                    byte[] data = Encoding.ASCII.GetBytes(result);
                    context.Response.ContentLength64 = data.Length;

                    using (Stream content = context.Response.OutputStream)
                        content.Write(data, 0, data.Length);
                }
                catch (ResponseNotFoundException)
                {
                    context.Response.StatusCode = 404;
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    Console.WriteLine(ex);
                }

                context.Response.Close();
            }
        }

        public static void StopListening()
        {
            if (!listener.IsListening) return;
            listener.Stop();
        }

        public static bool IsListening => listener.IsListening;
    }
}
