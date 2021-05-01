using EventBusRabbitMQ;

namespace POC.OpenTelemetry.API.Models
{
    public record UserAddedEvent: IntegrationEvent
    {
        public string Username { get; set; }
    }
}
