using Nybus.Policies;

namespace Nybus.Configuration
{
    public interface INybusBusOptionsBuilder
    {
        void SetCommandErrorPolicy<TPolicy>() where TPolicy : ICommandErrorPolicy;

        void SetEventErrorPolicy<TPolicy>() where TPolicy : IEventErrorPolicy;
    }
}