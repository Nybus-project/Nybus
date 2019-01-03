using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class BusSetupTests
    {
        [Test, AutoMoqData]
        public async Task Bus_can_invoke_command(ServiceCollection services, FirstTestCommand testCommand)
        {
            services.AddLogging();

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();
        }

        [Test, AutoMoqData]
        public async Task Bus_can_raise_events(ServiceCollection services, FirstTestEvent testEvent)
        {
            services.AddLogging();

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.RaiseEventAsync(testEvent);

            await host.StopAsync();
        }

        [Test, AutoMoqData]
        public async Task Host_can_receive_commands(ServiceCollection services, FirstTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceived<FirstTestCommand>>();

            services.AddLogging();

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToCommand(commandReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Host_can_receive_events(ServiceCollection services, FirstTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceived<FirstTestEvent>>();

            services.AddLogging();

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToEvent(eventReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }
    }
}