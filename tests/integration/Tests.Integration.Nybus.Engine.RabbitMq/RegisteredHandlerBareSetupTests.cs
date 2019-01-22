using System.Threading.Tasks;
using FakeRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using static Tests.TestUtils;

namespace Tests
{
    [TestFixture]
    public class RegisteredHandlerBareSetupTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, SecondTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceivedAsync<SecondTestCommand>>();
            var mockHandler = new Mock<SecondTestCommandHandler>(commandReceived);
            var handler = mockHandler.Object;

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq => { rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory()); });

                nybus.SubscribeToCommand<SecondTestCommand, SecondTestCommandHandler>();
            },
            services =>
            {
                services.AddSingleton(commandReceived);
                services.AddSingleton(handler);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<SecondTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, SecondTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceivedAsync<SecondTestEvent>>();
            var mockHandler = new Mock<SecondTestEventHandler>(eventReceived);
            var handler = mockHandler.Object;

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq => { rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory()); });

                nybus.SubscribeToEvent<SecondTestEvent, SecondTestEventHandler>();
            },
            services =>
            {
                services.AddSingleton(eventReceived);
                services.AddSingleton(handler);

            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<SecondTestEvent>>()));

        }

    }
}