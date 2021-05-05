using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using POC.OpenTelemetry.Grpc;
using POC.OpenTelemetry.Worker.Models;
using System;
using System.Diagnostics;

namespace POC.OpenTelemetry.Worker
{
    public class GrpcClient
    {
        public static ActivitySource GrpcUsersService = new ActivitySource(nameof(GrpcClient));
        private readonly IConfiguration _configuration;

        public GrpcClient(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void SendUser(UserAddedEvent user)
        {
            using var activity = GrpcUsersService.StartActivity($"{nameof(GrpcUsersService)} Create user", ActivityKind.Internal, Activity.Current.Context);

            AppContext.SetSwitch(
              "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",
              true);

            var host = _configuration["GrpcServiceHost"];

            var channel = GrpcChannel.ForAddress(host);
            var client = new Greeter.GreeterClient(channel);

            var grpcResponse = client.SayHelloAsync(new HelloRequest { Name = user.Username });

            activity.SetTag("Response", $"Grpc response: { grpcResponse }");
        }
    }
}
