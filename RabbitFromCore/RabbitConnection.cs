using System.Collections.Generic;
using System.IO;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMqTestMessageSender.RabbitFromCore
{
    internal class RabbitConnection : IRabbit
    {
        private readonly string _routingKey;

        private readonly ConnectionFactory _factory;
        private IModel _channel;
        private IConnection _connection;
        private bool _connected;

        public RabbitConnection(string routingKey)
        {
            _routingKey = routingKey;

            _factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost",
                Port = 5672,
                // AutomaticRecoveryEnabled = true,
                // RequestedConnectionTimeout = configuration.Timeout,
                // VirtualHost = configuration.VirtualHost,
            };
        }

        // Установить соединение с Rabbit. Выбросит исключение если не удастся соединиться.
        public void Reconnect()
        {
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                try
                {
                    _connection.Dispose();
                }
                catch (EndOfStreamException)
                {
                    // Этот деструктор может выбросить исключение EndOfStreamException.
                    // https://stackoverflow.com/questions/12499174/rabbitmq-c-sharp-driver-stops-receiving-messages
                }
                _connection = null;
            }

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_routingKey, true, false, false, null);
            _connected = true;
        }

        public void Publish(Dictionary<string, string> headers, string body)
        {
            if (!_connected)
            {
                Reconnect();
            }

            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent

            properties.Headers = new Dictionary<string, object>();
            if (headers != null)
                foreach (var entry in headers)
                    properties.Headers[entry.Key] = entry.Value;

            _channel.BasicPublish("", _routingKey, properties, body == null ? null : Encoding.UTF8.GetBytes(body));
        }
    }
}
