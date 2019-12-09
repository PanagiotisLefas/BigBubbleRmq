using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BigBubble.Abstractions;
using RabbitMQ.Client;

namespace BigBubble.App
{
    public class Publisher
    {
        private readonly IModel _publishChannel;
        private readonly string _queueName;
        private readonly IBasicProperties _basicProperties;


        public bool Joined { get; private set; }

        public Publisher(IRmqConnectionFactory connectionFactory, IUsernameProvider username)
        {


            _publishChannel = connectionFactory.GetPublishChannel();
            _queueName = _publishChannel.QueueDeclare().QueueName;

            _publishChannel.QueueBind(queue: _queueName,
                              exchange: "chat_fnt",
                              routingKey: "");
            _basicProperties = _publishChannel.CreateBasicProperties();
            _basicProperties.Persistent = true;
            _basicProperties.Headers = new Dictionary<string, object>();
            //_basicProperties.Headers.Add("nickname", username.GetUsername());
        }

        public void PublishMessage(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            var body = Encoding.UTF8.GetBytes(json);
            _publishChannel.BasicPublish(exchange: "chat_fnt",
                                 routingKey: "",
                                 basicProperties: _basicProperties,
                                 body: body);

        }


    }
}
