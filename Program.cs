using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Payler;
using MassTransit;
using RabbitMqTestMessageSender.RabbitFromCore;

namespace RabbitMqTestMessageSender
{
    internal static class Program
    {
        private const string QueueName = "payler-core-yandex-pay-notifications";
        private const string ExchangeName = "payler-core-yandex-pay-notifications-exchange";

        private const string RealMessageId =
            "1:gAAAAABhVuUF7mkqLPqp5AfT48Ns0pGuESogFo1G0DahVKpVLd1le1nVv3Gfykvm-pxxibnhTrjjsZtHcxHCZmheZn0t6FGrCWbmxjLWZDggL5dywfBmboQPqG9f_0uD7FPn_eYhrasg";

        private const string RealMessageIdConsoleKey = "++";
        private static readonly List<string> AcceptedConsoleKeys = new() { "+", "i", RealMessageIdConsoleKey };

        private static YandexNotification MakeMessage(bool useRealMessageId = false)
        {
            Random randomizer = new();
            return new YandexNotification
            {
                MessageId = useRealMessageId ? RealMessageId : Guid.NewGuid().ToString(),
                DealStatus = DealStatus.Charged,
                ReasonCode = null,
                Reason = null,
                RefundAmount = randomizer.Next(),
                Currency = "RUB",
                Rrn = randomizer.Next(0, 999999999).ToString().PadRight(12, '0'),
                Time = DateTime.Now,
                AuthCode = randomizer.Next(1000).ToString().PadLeft(3),
                OrderId = Guid.NewGuid().ToString(),
                Eci = Guid.NewGuid().ToString(),
                PaymentSystem = "Visa"
            };
        }

        private static string MakeIncorrectSerializedMessageFromCore()
        {
            var notification = new YandexMessageFromCore(true);
            return JsonSerializer.Serialize(notification);
        }

        private static string MakeSerializedMessageFromCore(DealStatus dealStatus)
        {
            var notification = new YandexMessageFromCore(dealStatus: dealStatus);
            return JsonSerializer.Serialize(notification);
        }

        private static async Task Main(string[] args)
        {
            const string rabbitUrl = "localhost";

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
            Console.Clear();
            PrintMan();
            // var endpoint = await busControl.GetSendEndpoint(new Uri($"queue:{QueueName}"));
            var key = Console.ReadLine();

            while (key != null && (AcceptedConsoleKeys.Contains(key) || key.StartsWith('c')))
            {
                Console.Clear();
                switch (key)
                {
                    case RealMessageIdConsoleKey:
                    case "+":
                        // Если введено ++, то используем реальный идентификатор сообщения
                        var message = MakeMessage(key == RealMessageIdConsoleKey);
                        Console.WriteLine("Generated message:");
                        Console.WriteLine(JsonSerializer.Serialize(message));
                        Console.WriteLine("Publishing message to the query...");
                        // await endpoint.Send(message/*, source.Token*/);
                        await busControl.Publish(message);
                        break;

                    case "i":
                        var incorrectMessage = MakeIncorrectSerializedMessageFromCore();
                        Console.WriteLine("Generated core message:");
                        Console.WriteLine(incorrectMessage);
                        Console.WriteLine("Publishing message to the query...");
                        RabbitNotificationManager.Instance
                                                 .Notify(QueueName, new Dictionary<string, string>(), incorrectMessage);
                        break;

                    default:
                        if (key.StartsWith('c'))
                        {
                            var defaultDealStatus = DealStatus.Charged;
                            var arguments = key.Split(' ');
                            if (arguments.Length > 1 && byte.TryParse(arguments[1], out var byteStatus))
                            {
                                defaultDealStatus = (DealStatus)byteStatus;
                            }

                            var coreMessage = MakeSerializedMessageFromCore(defaultDealStatus);
                            Console.WriteLine("Generated core message:");
                            Console.WriteLine(coreMessage);
                            Console.WriteLine("Publishing message to the query...");
                            RabbitNotificationManager.Instance
                                                     .Notify(QueueName, new Dictionary<string, string>(), coreMessage);
                        }

                        break;
                }

                Console.WriteLine();
                PrintMan();
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

        private static void PrintMan()
        {
            Console.WriteLine($"Type '+' to publish random {nameof(YandexNotification)} message using MT,");
            Console.WriteLine($"type '++' to publish random {nameof(YandexNotification)} " +
                              "message with real MessageId using MT,");
            Console.WriteLine($"type 'c' [status_num] to publish random {nameof(YandexMessageFromCore)} " +
                              "message using default client,");
            Console.Write($"type 'i' to publish incorrect {nameof(YandexMessageFromCore)} " +
                          "using default client to the RabbitMQ: ");
        }
    }
}