using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nybus
{
    public static class DispatcherExtensions
    {
        private static readonly IDictionary<string, string> EmptyHeaders = new Dictionary<string, string>();

        public static Task InvokeCommandAsync<TCommand>(this IDispatcher dispatcher, TCommand command)
            where TCommand : class, ICommand
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            return dispatcher.InvokeCommandAsync(command, EmptyHeaders);
        }

        public static async Task InvokeManyCommandsAsync<TCommand>(this IDispatcher dispatcher, IEnumerable<TCommand> commands)
            where TCommand : class, ICommand
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(c => dispatcher.InvokeCommandAsync(c, EmptyHeaders))).ConfigureAwait(false);
        }

        public static async Task InvokeManyCommandsAsync<TCommand>(this IDispatcher dispatcher, IEnumerable<TCommand> commands, IDictionary<string, string> headers)
            where TCommand : class, ICommand
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(c => dispatcher.InvokeCommandAsync(c, headers))).ConfigureAwait(false);
        }

        public static Task RaiseEventAsync<TEvent>(this IDispatcher dispatcher, TEvent @event)
            where TEvent : class, IEvent
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            return dispatcher.RaiseEventAsync(@event, EmptyHeaders);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IDispatcher dispatcher, IEnumerable<TEvent> events)
            where TEvent : class, IEvent
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(e => dispatcher.RaiseEventAsync(e, EmptyHeaders))).ConfigureAwait(false);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IDispatcher dispatcher, IEnumerable<TEvent> events, IDictionary<string, string> headers)
            where TEvent : class, IEvent
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(e => dispatcher.RaiseEventAsync(e, headers))).ConfigureAwait(false);
        }
    }
}