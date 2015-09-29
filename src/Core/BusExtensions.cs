using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public static class BusExtensions
    {
        public static async Task InvokeManyCommands<TCommand>(this IBus bus, IEnumerable<TCommand> commands)
            where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(bus.InvokeCommand)).ConfigureAwait(false);
        }

        public static async Task RaiseManyEvents<TEvent>(this IBus bus, IEnumerable<TEvent> events)
            where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(bus.RaiseEvent)).ConfigureAwait(false);
        }
    }
}
