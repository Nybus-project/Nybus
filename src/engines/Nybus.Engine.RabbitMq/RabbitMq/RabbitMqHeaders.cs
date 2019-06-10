namespace Nybus.RabbitMq
{
    public static class RabbitMqHeaders
    {
        public static readonly string MessageId = "RabbitMq:MessageId";
        public static readonly string DeliveryTag = "RabbitMq:DeliveryTag";

        public static bool IsRabbitMq(string key) => key.StartsWith("RabbitMq");
    }
}
