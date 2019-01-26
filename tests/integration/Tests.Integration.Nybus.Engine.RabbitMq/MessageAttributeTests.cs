using System.Threading.Tasks;
using FakeRabbitMQ;
using Moq;
using NUnit.Framework;
using Nybus;
using Samples;
using static Tests.TestUtils;

namespace Tests
{
    [TestFixture]
    public class MessageAttributeTests
    {
        [Test, AutoMoqData]
        public async Task Commands_are_matched_via_MessageAttribute(FakeServer server, ThirdTestCommand testCommand, CommandReceivedAsync<AttributeTestCommand> commandReceived)
        {
            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<AttributeTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Events_are_matched_via_MessageAttribute(FakeServer server, ThirdTestEvent testEvent, EventReceivedAsync<AttributeTestEvent> eventReceived)
        {
            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<AttributeTestEvent>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Commands_are_correctly_converted(FakeServer server, ThirdTestCommand testCommand, CommandReceivedAsync<AttributeTestCommand> commandReceived)
        {
            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<AttributeTestCommand>>(c => string.Equals(c.Command.Message, testCommand.Message))), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Events_are_correctly_converted(FakeServer server, ThirdTestEvent testEvent, EventReceivedAsync<AttributeTestEvent> eventReceived)
        {
            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<AttributeTestEvent>>(e => string.Equals(e.Event.Message, testEvent.Message))), Times.Once);
        }
    }
}
