using EventBusRabbitMQ;

namespace POC.OpenTelemetry.API.Models
{
    public record AddUser: IntegrationEvent
    {
        public string Username { get; set; }
    }
}
