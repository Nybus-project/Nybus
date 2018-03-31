using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nybus {
    public static class DispatcherExtensions
    {
        public static async Task InvokeManyCommandsAsync<TCommand>(this IDispatcher dispatcher, IEnumerable<TCommand> commands)
            where TCommand : class, ICommand
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(dispatcher.InvokeCommandAsync)).ConfigureAwait(false);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IDispatcher dispatcher, IEnumerable<TEvent> events)
            where TEvent : class, IEvent
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(dispatcher.RaiseEventAsync)).ConfigureAwait(false);
        }
    }
}