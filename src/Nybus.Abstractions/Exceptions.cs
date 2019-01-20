using System;

namespace Nybus
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
            
        }

        public ConfigurationException(string message) : base (message)
        {
            
        }
    }

    public class MissingHandlerException : Exception
    {
        public MissingHandlerException(Type handlerType, string message) : base(message)
        {
            HandlerType = handlerType;
        }

        public Type HandlerType { get; }
    }
}