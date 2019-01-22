using System.Threading.Tasks;
using FakeRabbitMQ;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests {
    [TestFixture]
    public class SynchronousDelegateHandlerTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, FirstTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceived<FirstTestCommand>>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(nybus,
                    rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });

                NybusConfiguratorExtensions.SubscribeToCommand(nybus, commandReceived);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, FirstTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceived<FirstTestEvent>>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(nybus,
                    rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });

                NybusConfiguratorExtensions.SubscribeToEvent(nybus, eventReceived);
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()));
        }

    }
}