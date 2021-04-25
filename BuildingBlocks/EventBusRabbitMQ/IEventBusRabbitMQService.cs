namespace EventBusRabbitMQ
{
    public interface IEventBusRabbitMQService
    {
        void Publish(IntegrationEvent @event);
    }
}
