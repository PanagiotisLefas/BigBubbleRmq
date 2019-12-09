using RabbitMQ.Client;

namespace BigBubble.Abstractions
{
    public interface IRmqConnectionFactory
    {
        IModel GetConsumeChannel();
        IModel GetPublishChannel();

    }

    public interface IUsernameProvider
    {
        string GetUsername();
        void SetUsername(string username);
    }
}
