using System;

namespace Nybus.Configuration
{
    public interface IBusBuilder<out TConfiguration> where TConfiguration : IBusConfiguration
    {
        IBus Build(Action<TConfiguration> configurator);
    }
}