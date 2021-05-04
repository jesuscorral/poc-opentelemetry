using EventBusRabbitMQ;

namespace POC.OpenTelemetry.Worker.Models
{
    public record UserAddedEvent : IntegrationEvent
    {
        public string Username { get; set; }
    }
}
