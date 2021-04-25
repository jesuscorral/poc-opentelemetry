using EventBusRabbitMQ;

namespace POC.OpenTelemetry.WorkerService.Models
{
    public record AddUser : IntegrationEvent
    {
        public string Username { get; set; }
    }
}
