using System;
using BigBubble.Abstractions;
using RabbitMQ.Client;

namespace BigBubble.Services
{
    public class RmqConnectionFactory : IRmqConnectionFactory, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _consumerChannel;
        private readonly IModel _publishChannel;

        public RmqConnectionFactory(string hostName)
        {
            var factory = new ConnectionFactory() { HostName = hostName };
            _connection = factory.CreateConnection();
            _consumerChannel = _connection.CreateModel();
            _publishChannel = _connection.CreateModel();


        }

        public IModel GetConsumeChannel()
        {
            return _consumerChannel;
        }

        public IModel GetPublishChannel()
        {
            return _publishChannel;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _consumerChannel.Dispose();
                    _publishChannel.Dispose();
                    _connection.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
