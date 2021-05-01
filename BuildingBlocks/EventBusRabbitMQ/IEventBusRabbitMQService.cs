using System.Threading.Tasks;

namespace EventBusRabbitMQ
{
    public interface IEventBusRabbitMQService
    {
        Task Publish(IntegrationEvent @event);

        void CloseConnection();
    }
}
