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
        private readonly ILogger<GreeterService> _logger;
        private readonly IHttpClientFactory _httpClient;

        public GreeterService(ILogger<GreeterService> logger,
            IHttpClientFactory httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Activity.Current.SetTag("UserRequest", JsonSerializer.Serialize(request));

            var ret = Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });

            var client = _httpClient.CreateClient();
            var response = await client.PostAsync("https://poc-opentelemetry.requestcatcher.com/", 
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

            return await ret;
        }
    }
}
