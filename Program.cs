using System;
using Domain.Entities.Payler;
using EasyNetQ;
using EasyNetQ.Topology;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RabbitMqTestMessageSender
{
    internal static class Program
    {
        private static PaymentInfoMessage MakeMessage()
        {
            var randomizer = new Random();

            return new PaymentInfoMessage
            {
                YandexMessageId = Guid.NewGuid().ToString(),
                DealStatus = DealStatus.Charged, 
                YandexReasonCode = null,
                Reason = null,
                MerchantName = $"Test {randomizer.Next(1001)}",
                Amount = randomizer.Next(),
                Currency = "RUB",
                CardNumber = $"{randomizer.Next(10000).ToString(),4}xxxxxxxx{randomizer.Next(10000).ToString(),4}",
                Kind = "Payment",
                Product = $"Product {randomizer.Next()}",
                Rrn = "1234567890",
                Time = DateTime.Now, //.ToString("dd.MM.yyyy HH:mm:ss"),
                AuthCode = randomizer.Next(1000).ToString().PadLeft(3),
                OrderId = Guid.NewGuid().ToString(),
                CardHolderName = "Card Holder",
                UserEnteredEmail = "holder@mail.domain"
            };
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("RabbitMQ bus preparation...");

            // var bus = RabbitHutch.CreateBus("host=localhost");
            Console.Write("Press '+' to publish random message to the RabbitMQ: ");
            var key = Console.ReadLine();
            // while (key == "+")
            // {
            //     Console.Clear();
            //     var message = MakeMessage();
            //     Console.WriteLine("Generated message:");
            //     Console.WriteLine(JsonSerializer.Serialize(message));
            //     Console.WriteLine("Publishing message to the query...");
            //
            //     bus.PubSub.Publish(message);
            //     //    .ContinueWith(task =>
            //     // {
            //     //     // this only checks that the task finished
            //     //     // IsCompleted will be true even for tasks in a faulted state
            //     //     // we use if (task.IsCompleted && !task.IsFaulted) to check for success
            //     //     if (task.IsCompleted) 
            //     //     {
            //     //         Console.WriteLine("{0} Completed", task.Id);
            //     //     }
            //     //     if (task.IsFaulted)
            //     //     {
            //     //         Console.WriteLine(task.Exception);
            //     //     }
            //     // });
            //
            //     Console.Write("Publish another message? ");
            //     key = Console.ReadLine();
            // }

            using (var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest").Advanced)
            {
                var exchange = bus.ExchangeDeclare("YandexPay", ExchangeType.Topic);
                var queue = bus.QueueDeclare("YandexPay");
                bus.Bind(exchange, queue, "YandexPay");
                while (key == "+")
                {
                    Console.Clear();
                    var message = MakeMessage();
                    Console.WriteLine("Generated message:");
                    Console.WriteLine(JsonSerializer.Serialize(message));
                    Console.WriteLine("Publishing message to the query...");
                    bus.Publish(exchange, "YandexPay", true, new Message<PaymentInfoMessage>(message));
            
                    Console.Write("Publish another message? ");
                    key = Console.ReadLine();
                }
            
                Console.WriteLine("Disposing bus...");
            }

            // Console.WriteLine("Disposing bus...");
            Console.WriteLine("Application finished");
        }
    }
}