using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using PipelineProcessor2.Server.Exceptions;
using PipelineProcessor2.Server.Responses;

namespace PipelineProcessor2.Server
{
    class ResponseFactory
    {
        Dictionary<string, IResponse> responses = new Dictionary<string, IResponse>();

        public ResponseFactory()
        {
            Console.WriteLine("Detecting Endpoints");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    try
                    {
                        if (typeof(IResponse).IsAssignableFrom(type) && !type.IsInterface)
                        {
                            IResponse response = (IResponse)Activator.CreateInstance(type);
                            responses.Add(response.EndpointLocation(), response);

                            Console.WriteLine("Added Endpoint: " + response.EndpointLocation());
                        }
                    }
                    catch (InvalidCastException) { } //ignore
                }
            }
        }

        public string BuildResponse(HttpListenerRequest request)
        {
            if (responses.ContainsKey(request.RawUrl))
                return responses[request.RawUrl].Response(request);

            throw new ResponseNotFoundException();
        }
    }
}
