using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Nybus.RabbitMq
{
    public static class BasicPropertiesExtensions
    {
        public static string GetHeader(this IBasicProperties properties, string headerName, Encoding encoding)
        {
            if (properties.Headers.TryGetValue(headerName, out var value))
            {
                if (value is byte[] bytes)
                {
                    return encoding.GetString(bytes);
                }

                if (value is string str)
                {
                    return str;
                }
            }

            return null;
        }
    }
}
