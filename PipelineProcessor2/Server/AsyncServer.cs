using System;
using System.Diagnostics;
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
            if (responses == null) responses = new ResponseFactory();

            if (listener.IsListening) listener.Stop();
            listener.Prefixes.Add("http://localhost:" + Port + "/");
            listener.Prefixes.Add("http://127.0.0.1:" + Port + "/");


            try
            {
                listener.Start();
            }
            catch (HttpListenerException hle)
            {
                Console.WriteLine("Could not start listening as target address are already in use!");
                return;
            }

            listenThread = new Task(Listen);
            listenThread.Start();
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
                    string result;
                    if(context.Request.Url.AbsolutePath.StartsWith("/api"))
                        result = responses.BuildResponse(context.Request);
                    else result = ServeFile(context);

                    byte[] data = Encoding.ASCII.GetBytes(result);
                    context.Response.ContentLength64 = data.Length;

                    using(Stream content = context.Response.OutputStream)
                        content.Write(data, 0, data.Length);
                }
                catch(FileNotFoundException e)
                {
                    context.Response.StatusCode = 404;
                    Console.WriteLine(e.Message);
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

        private static string ServeFile(HttpListenerContext request)
        {
            string basePath = Directory.GetCurrentDirectory() + "\\Server\\WebComponents\\";
            string filePath;

            if(request.Request.Url.AbsolutePath == "/") filePath = basePath + "index.html";
            else filePath = basePath + request.Request.Url.AbsolutePath.Replace("/", "\\");

            if(!File.Exists(filePath)) throw new FileNotFoundException("Cannot find web resources for: " + request.Request.Url.AbsolutePath);
            if(filePath.EndsWith(".css")) request.Response.ContentType = "text/css";

            return File.ReadAllText(filePath);
        }

        public static bool IsListening => listener.IsListening;
    }
}
