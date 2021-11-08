namespace RabbitMqTestMessageSender.RabbitFromCore
{
    /// <summary>
    /// Описания триггеров, обрабатываемых системой уведомлений Payler
    /// </summary>
    public enum PaylerNotificationKind
    {
        /// <summary>
        /// Средства заблокированы.
        /// </summary>
        Block,

        /// <summary>
        /// Средства разблокированы.
        /// </summary>
        Unblock,

        /// <summary>
        /// Успешно произведен полный возврат денежных средств на карту пользователя.
        /// </summary>
        Refund,

        /// <summary>
        /// Денежные средства списаны с карты пользователя, платёж завершен успешно.
        /// </summary>
        Charge,

        /// <summary>
        /// Осуществлен повторяющийся платеж.
        /// </summary>
        Repeatpay,

        /// <summary>
        /// Успешно произведен перевод денежных средств.
        /// </summary>
        Credit,

        /// <summary>
        /// Чек. Отправляется не ядром, а отдельным сервисом.
        /// </summary>
        Receipt,

        /// <summary>
        /// Запрос к мерчанту при методе оплаты checkout. Выполняется напрямую, не через очередь
        /// </summary>
        Check,

        /// <summary>
        /// Платеж отклонен
        /// </summary>
        Declined,

        /// <summary>
        /// Карта сохранена
        /// </summary>
        CardSaved,
    }
}
