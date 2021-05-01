using EventBusRabbitMQ;

namespace POC.OpenTelemetry.API.Models
{
    public record UserAdded: IntegrationEvent
    {
        public string Username { get; set; }
    }
}
