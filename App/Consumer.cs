using System.Text;
using System.Text.Json;
using BigBubble.Abstractions;
using BigBubble.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BigBubble.App
{
    public class Consumer
    {
        private readonly IModel _consumerChannel;
        private readonly IUsernameProvider _username;
        private string _queueName;

        public event ConsumerMessageReceivedEventHandler MessageReceived;


        public Consumer(IRmqConnectionFactory connectionFactory, IUsernameProvider username)
        {
            _consumerChannel = connectionFactory.GetPublishChannel();
            _username = username;
        }

        public void InitConsume()
        {
            _queueName = _consumerChannel.QueueDeclare().QueueName;
            _consumerChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _consumerChannel.QueueBind(queue: _queueName,
                              exchange: "chat_fnt",
                              routingKey: "");

            var consumer = new EventingBasicConsumer(_consumerChannel);
            consumer.Received += (model, ea) =>
            {

                MessageReceived?.Invoke(this, DecodeMesage(ea.Body));

                _consumerChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            _consumerChannel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        private MessageReceivedModel DecodeMesage(byte[] body)
        {
            var json = Encoding.UTF8.GetString(body);
            MessageReceivedModel sanitized = null;

            try
            {
                var message = JsonSerializer.Deserialize<MessageReceivedModel>(json);
                sanitized = message.Sanitize();
            }
            catch
            {
            }

            try
            {
                var messageBasic = JsonSerializer.Deserialize<MessageReceivedModelBase>(json);
                var message = new MessageReceivedModel()
                {
                    Message = messageBasic.Message,
                    Nickname = messageBasic.Nickname,
                    Timestamp = messageBasic.Received,
                    Type = messageBasic.Type
                };

                sanitized = message.Sanitize();
            }
            catch
            {
            }

            return sanitized;
        }


    }
}
