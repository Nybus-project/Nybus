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
        public async Task Host_can_loopback_commands(FakeServer server, FirstTestCommand testCommand, CommandReceived<FirstTestCommand> commandReceived)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.SubscribeToCommand(commandReceived);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, FirstTestEvent testEvent, EventReceived<FirstTestEvent> eventReceived)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.SubscribeToEvent(eventReceived);
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()));
        }

    }
}