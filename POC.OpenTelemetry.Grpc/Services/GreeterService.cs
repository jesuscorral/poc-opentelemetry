using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.Grpc
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly IHttpClientFactory _httpClient;

        public GreeterService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            try
            {
                Activity.Current.SetTag("UserRequest", JsonSerializer.Serialize(request));

                // Web hook request
                var client = _httpClient.CreateClient();
                var response = await client.PostAsync("https://poc-opentelemetry.requestcatcher.com/",
                    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

                Activity.Current.SetTag("Webhook", "Webhook called");

                return new HelloReply
                {
                    Message = "Hello " + request.Name
                };
            }
            catch(Exception ex)
            {
                Activity.Current.SetTag("Error", ex.Message);
            }
            return null;
        }
    }
}
