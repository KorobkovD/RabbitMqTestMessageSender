using System.Collections.Generic;

namespace RabbitMqTestMessageSender.RabbitFromCore
{
    public interface IRabbit
    {
        void Publish(Dictionary<string, string> headers, string body);
    }
}
