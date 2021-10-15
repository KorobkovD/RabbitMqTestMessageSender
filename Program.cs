using System;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Payler;
using MassTransit;

namespace RabbitMqTestMessageSender
{
    internal static class Program
    {
        private const string QueueName = "payler-core-yandex-pay-notifications";
        private const string ExchangeName = "payler-core-yandex-pay-notifications-exchange";

        private static YandexNotification MakeMessage()
        {
            var randomizer = new Random();

            return new YandexNotification
            {
                MessageId = Guid.NewGuid().ToString(),
                DealStatus = DealStatus.Charged,
                ReasonCode = null,
                Reason = null,
                RefundAmount = randomizer.Next(),
                Currency = "RUB",
                Rrn = "1234567890",
                Time = DateTime.Now, //.ToString("dd.MM.yyyy HH:mm:ss"),
                AuthCode = randomizer.Next(1000).ToString().PadLeft(3),
                OrderId = Guid.NewGuid().ToString(),
                Eci = Guid.NewGuid().ToString(),
                PaymentSystem = "Visa"
            };
        }

        private static SomeUnexpectedMessage MakeUnexpectedMessage()
        {
            return new SomeUnexpectedMessage
            {
                Date = DateTime.Now,
                Message = "Oops, that was unexpected..."
            };
        }

        private static async Task Main(string[] args)
        {
            var rabbitUrl = "localhost";

            Console.WriteLine("RabbitMQ bus preparation...");

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(rabbitUrl, configurator =>
                {
                    configurator.Username("guest");
                    configurator.Password("guest");
                });
                cfg.Publish<YandexNotification>(conf =>
                {
                     // conf.BindQueue(ExchangeName, QueueName);
                });
            });

            await busControl.StartAsync( /*source.Token*/);
            Console.WriteLine($"Press '+' to publish random {nameof(YandexNotification)} message,");
            Console.Write($"press 'u' to publish {nameof(SomeUnexpectedMessage)} to the RabbitMQ: ");
            // var endpoint = await busControl.GetSendEndpoint(new Uri($"queue:{QueueName}"));
            var key = Console.ReadLine();

            while (key is "+" or "u")
            {
                Console.Clear();
                switch (key)
                {
                    case "+":
                        var message = MakeMessage();
                        Console.WriteLine("Generated message:");
                        Console.WriteLine(JsonSerializer.Serialize(message));
                        Console.WriteLine("Publishing message to the query...");
                        // await endpoint.Send(message/*, source.Token*/);
                        await busControl.Publish(message);
                        break;
                    case "u":
                        var unexpectedMessage = MakeUnexpectedMessage();
                        Console.WriteLine("Generated unexpected message:");
                        Console.WriteLine(JsonSerializer.Serialize(unexpectedMessage));
                        Console.WriteLine("Publishing message to the query...");
                        // await endpoint.Send(unexpectedMessage/*, source.Token*/);
                        await busControl.Publish(unexpectedMessage);
                        break;
                }

                Console.Write("Publish another message? ");
                key = Console.ReadLine();
            }

            Console.WriteLine("Disposing bus...");
            await busControl.StopAsync( /*source.Token*/);
            Console.WriteLine("Application finished");

            // using (var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest").Advanced)
            // {
            //     var exchange = bus.ExchangeDeclare(QueueName, ExchangeType.Topic);
            //     var queue = bus.QueueDeclare(QueueName);
            //     bus.Bind(exchange, queue, QueueName);
            //     Console.WriteLine($"Press '+' to publish random {nameof(YandexNotification)} message,");
            //     Console.Write($"press 'u' to publish {nameof(SomeUnexpectedMessage)} to the RabbitMQ: ");
            //     var key = Console.ReadLine();
            //     while (key is "+" or "u")
            //     {
            //         Console.Clear();
            //         switch (key)
            //         {
            //             case "+":
            //                 var message = MakeMessage();
            //                 Console.WriteLine("Generated message:");
            //                 Console.WriteLine(JsonSerializer.Serialize(message));
            //                 Console.WriteLine("Publishing message to the query...");
            //                 bus.Publish(exchange, QueueName, true, new Message<YandexNotification>(message));
            //                 break;
            //             case "u":
            //                 var unexpectedMessage = MakeUnexpectedMessage(); 
            //                 Console.WriteLine("Generated unexpected message:");
            //                 Console.WriteLine(JsonSerializer.Serialize(unexpectedMessage));
            //                 Console.WriteLine("Publishing message to the query...");
            //                 bus.Publish(exchange, QueueName, true, new Message<SomeUnexpectedMessage>(unexpectedMessage));
            //                 break;
            //         }
            //
            //         Console.Write("Publish another message? ");
            //         key = Console.ReadLine();
            //     }
            //
            //     Console.WriteLine("Disposing bus...");
            // }

            // Console.WriteLine("Disposing bus...");
            // Console.WriteLine("Application finished");
        }
    }
}