using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class HeaderTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(ServiceCollection services, SecondTestCommand testCommand, CommandReceivedAsync<SecondTestCommand> commandReceived, string headerKey, string headerValue)
        {
            services.AddLogging(l => l.AddDebug());

            services.AddSingleton(commandReceived);
            services.AddSingleton<ICommandHandler<SecondTestCommand>, SecondTestCommandHandler>();

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToCommand<SecondTestCommand>();
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            var headers = new Dictionary<string, string>
            {
                [headerKey] = headerValue
            };

            await bus.InvokeCommandAsync(testCommand, headers);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<SecondTestCommand>>(c => c.Message.Headers.ContainsKey(headerKey) && c.Message.Headers[headerKey] == headerValue)));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(ServiceCollection services, SecondTestEvent testEvent, EventReceivedAsync<SecondTestEvent> eventReceived, string headerKey, string headerValue)
        {
            services.AddLogging(l => l.AddDebug());

            services.AddSingleton(eventReceived);
            services.AddSingleton<IEventHandler<SecondTestEvent>, SecondTestEventHandler>();

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToEvent<SecondTestEvent>();
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            var headers = new Dictionary<string, string>
            {
                [headerKey] = headerValue
            };

            await bus.RaiseEventAsync(testEvent, headers);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<SecondTestEvent>>(c => c.Message.Headers.ContainsKey(headerKey) && c.Message.Headers[headerKey] == headerValue)));
        }

    }
}