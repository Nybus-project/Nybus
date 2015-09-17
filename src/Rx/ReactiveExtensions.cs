using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Nybus.Configuration;

namespace Nybus
{
    public static class ReactiveExtensions
    {
        public static IObservable<CommandContext<TCommand>> ObserveCommand<TCommand>(this IBusBuilder builder) 
            where TCommand : class, ICommand
        {
            ISubject<CommandContext<TCommand>> subject = new Subject<CommandContext<TCommand>>();

            builder.SubscribeToCommand<TCommand>(msg =>
            {
                subject.OnNext(msg);
                return Task.CompletedTask;
            });

            return subject;
        }

        public static IObservable<TCommand> Command<TCommand>(this IObservable<CommandContext<TCommand>> observable)
            where TCommand : class, ICommand
        {
            return observable.Select(msg => msg.Message);
        }

        public static IObservable<EventContext<TEvent>> ObserveEvent<TEvent>(this IBusBuilder builder)
            where TEvent : class, IEvent
        {
            ISubject<EventContext<TEvent>> subject = new Subject<EventContext<TEvent>>();

            builder.SubscribeToEvent<TEvent>(msg =>
            {
                subject.OnNext(msg);
                return Task.CompletedTask;
            });

            return subject;
        }

        public static IObservable<TEvent> Event<TEvent>(this IObservable<EventContext<TEvent>> observable)
            where TEvent : class, IEvent
        {
            return observable.Select(msg => msg.Message);
        }
    }
}
