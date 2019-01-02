using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nybus;
using Nybus.Configuration;
using Nybus.Policies;
using Nybus.Utils;

namespace Tests
{
    public class FirstTestCommand : ICommand
    {
        public string Message { get; set; }
    }

    public class FirstTestCommandHandler : ICommandHandler<FirstTestCommand>
    {
        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<FirstTestCommand> incomingCommand)
        {
            throw new NotImplementedException();
        }
    }

    public class SecondTestCommand : ICommand { }

    public class FirstTestEvent : IEvent { }

    public class FirstTestEventHandler : IEventHandler<FirstTestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<FirstTestEvent> incomingEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class SecondTestEvent : IEvent { }

    public class TestClock : IClock
    {
        public TestClock(DateTimeOffset initialTime)
        {
            Now = initialTime;
        }

        public TestClock() : this(DateTimeOffset.UtcNow)
        {

        }

        public DateTimeOffset Now { get; private set; }

        public void AdvanceBy(TimeSpan interval)
        {
            Now += interval;
        }

        public void SetTo(DateTimeOffset newValue)
        {
            Now = newValue;
        }
    }

    public class TestNybusConfigurator : INybusConfigurator
    {
        public void UseConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration, string sectionName = "Nybus")
        {
            Configuration = configuration.GetSection(sectionName);
        }

        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new List<Action<IServiceCollection>>();

        public void AddServiceConfiguration(Action<IServiceCollection> configurator)
        {
            _serviceConfigurations.Add(configurator);
        }

        public void ApplyServiceConfigurations(IServiceCollection services)
        {
            foreach (var sc in _serviceConfigurations)
                sc(services);
        }

        private readonly List<Action<ISubscriptionBuilder>> _subscriptionBuilders = new List<Action<ISubscriptionBuilder>>();

        public void AddSubscription(Action<ISubscriptionBuilder> configurator)
        {
            _subscriptionBuilders.Add(configurator);
        }

        public void ApplySubscriptions(ISubscriptionBuilder builder)
        {
            foreach (var sb in _subscriptionBuilders)
                sb(builder);
        }

        private Func<IServiceProvider, IErrorPolicy> _policyGenerator;

        public void SetErrorPolicy(Func<IServiceProvider, IErrorPolicy> policyGenerator)
        {
            _policyGenerator = policyGenerator;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; private set; }

        public void Configure(Action<INybusConfiguration> configuration)
        {
            throw new NotImplementedException();
        }
    }

}
