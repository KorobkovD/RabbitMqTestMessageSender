using System;
using Domain.Entities.Payler;

namespace RabbitMqTestMessageSender
{
    public class YandexMessageFromCore
    {
        public YandexMessageFromCore(bool shouldBeIncorrect = false, DealStatus dealStatus = DealStatus.Charged)
        {
            var randomizer = new Random();

            OrderId = Guid.NewGuid().ToString();
            DealStatus = shouldBeIncorrect ? (DealStatus)777 : dealStatus;
            Currency = "RUB";
            PaymentSystem = "VISA";
            Rrn = randomizer.Next(0, 999999999).ToString().PadRight(12, '0');
            AuthCode = randomizer.Next(10000000).ToString().PadLeft(6, '0');
            Amount = randomizer.Next(100, 100000);
            Reason = dealStatus == DealStatus.Rejected ? "Payment failed" : string.Empty;
            ReasonCode = dealStatus == DealStatus.Rejected ? "REJECTED" : string.Empty;
            Eci = randomizer.Next() % 2 == 0 ? randomizer.Next(1, 9).ToString().PadLeft(2, '0') : null;
            MessageId = Guid.NewGuid().ToString();
            Time = DateTime.Now;
        }

        /// <summary>
        /// Идентификатор сообщения из расшифрованного PaymentToken
        /// </summary>
        public string MessageId { get; set; }

        public string OrderId { get; set; }

        public DateTime Time { get; set; }

        /// <summary>
        /// Статус платежа
        /// </summary>
        public DealStatus DealStatus { get; set; }
        // public string DealStatus { get; set; }

        /// <summary>
        /// Код ошибки для обновления статуса платежа в YandexPay
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        /// Описание причины возникновения ошибки, если она была
        /// </summary>
        public string Reason { get; set; }

        public int? Amount { get; set; }

        public string Currency { get; set; }

        // "VISA" | "MASTERCARD" | "MIR"
        public string PaymentSystem { get; set; }

        public string Eci { get; set; }

        public string Rrn { get; set; }

        public string AuthCode { get; set; }
    }
}