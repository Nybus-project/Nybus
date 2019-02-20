using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Samples;

namespace Tests
{
    [TestFixture]
    public class MessageAttributeTests
    {
        [Test, AutoMoqData]
        public async Task Commands_are_matched_via_MessageAttribute(ServiceCollection services, ThirdTestCommand testCommand, CommandReceivedAsync<AttributeTestCommand> commandReceived)
        {
            services.AddLogging(l => l.AddDebug());

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

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<AttributeTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Outgoing_commands_are_marked_via_MessageAttribute(ServiceCollection services, AttributeTestCommand testCommand, CommandReceivedAsync<ThirdTestCommand> commandReceived)
        {
            services.AddLogging(l => l.AddDebug());

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

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<ThirdTestCommand>>()), Times.Once);

        }

        [Test, AutoMoqData]
        public async Task Events_are_matched_via_MessageAttribute(ServiceCollection services, ThirdTestEvent testEvent, EventReceivedAsync<AttributeTestEvent> eventReceived)
        {
            services.AddLogging(l => l.AddDebug());

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

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<AttributeTestEvent>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Outgoing_events_are_marked_via_MessageAttribute(ServiceCollection services, AttributeTestEvent testEvent, EventReceivedAsync<ThirdTestEvent> eventReceived)
        {
            services.AddLogging(l => l.AddDebug());

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

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<ThirdTestEvent>>()), Times.Once);

        }

        [Test, AutoMoqData]
        public async Task Commands_are_correctly_converted(ServiceCollection services, ThirdTestCommand testCommand, CommandReceivedAsync<AttributeTestCommand> commandReceived)
        {
            services.AddLogging(l => l.AddDebug());

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

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<AttributeTestCommand>>(c => string.Equals(c.Command.Message, testCommand.Message))), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Events_are_correctly_converted(ServiceCollection services, ThirdTestEvent testEvent, EventReceivedAsync<AttributeTestEvent> eventReceived)
        {
            services.AddLogging(l => l.AddDebug());

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

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<AttributeTestEvent>>(e => string.Equals(e.Event.Message, testEvent.Message))), Times.Once);
        }
    }
}
