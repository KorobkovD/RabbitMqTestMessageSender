using System;
using System.Collections.Generic;

namespace RabbitMqTestMessageSender.RabbitFromCore
{
    public class RabbitNotificationManager
    {
        public static RabbitNotificationManager Instance = new();

        // Словарь соединений с Рэббитом
        // Ключ - очередь для сообщений в RabbitMQ.
        // Список очередей в документации https://drive.google.com/open?id=1lOjReawn44nT1x9bAGIVh_b6iAuFsQZHhuiwJp0-t7A&authuser=1
        private readonly Dictionary<string, IRabbit> _rabbits;

        private RabbitNotificationManager()
        {
            NotificationsEnabled = !string.IsNullOrEmpty("localhost");

            _rabbits = new Dictionary<string, IRabbit>();
        }

        public RabbitNotificationManager(Dictionary<string, IRabbit> rabbits)
        {
            NotificationsEnabled = !string.IsNullOrEmpty("localhost");

            _rabbits = rabbits;
        }

        /// <summary>
        /// Доступность сервиса уведомлений
        /// </summary>
        private bool NotificationsEnabled { get; }

        // Получить соединение по имени очереди.
        // Создаёт новое или возвращает имеющееся.
        private IRabbit GetConnection(string routingKey)
        {
            lock (_rabbits)
            {
                if (_rabbits.ContainsKey(routingKey))
                    return _rabbits[routingKey];

                var rabbit = new RabbitConnection(routingKey);

                try
                {
                    rabbit.Reconnect();
                }
                catch (Exception)
                {
                    Console.WriteLine("[FAIL] Can't connect to RabbitMQ server");

                    // Не удалось соединится, но мы продолжаем как ни в чём не бывало,
                    // попробуем соединится при отправке следующего сообщения
                }

                _rabbits[routingKey] = rabbit;

                return rabbit;
            }
        }

        public void Notify(string queue, Dictionary<string, string> headers, string body)
        {
            if (!NotificationsEnabled)
            {
                return;
            }

            try
            {
                var rabbit = GetConnection(queue);
                lock (rabbit)
                {
                    rabbit.Publish(headers, body);
                }

                Console.WriteLine($"Sent event to RabbitMQ server {body}:");
            }
            catch (Exception)
            {
                Console.WriteLine("Can't send notification to RabbitMQ server");
            }
        }
    }
}