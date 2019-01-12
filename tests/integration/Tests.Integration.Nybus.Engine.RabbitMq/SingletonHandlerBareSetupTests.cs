using System.Threading.Tasks;
using FakeRabbitMQ;
using Moq;
using NUnit.Framework;
using Nybus;
using static Tests.TestUtils;

namespace Tests
{
    [TestFixture]
    public class SingletonHandlerBareSetupTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, FirstTestCommand testCommand)
        {
            var handler = Mock.Of<FirstTestCommandHandler>();

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(handler);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(handler).Verify(p => p.HandleAsync(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, FirstTestEvent testEvent)
        {
            var handler = Mock.Of<FirstTestEventHandler>();

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(handler);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(handler).Verify(p => p.HandleAsync(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }

    }
}