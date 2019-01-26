using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;
using Samples;

namespace Tests.External
{
    [ExternalTestFixture]
    public class MessageAttributeTests
    {
        [TearDown]
        public void OnTestComplete()
        {
            var connectionFactory = new ConnectionFactory();
            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();

            model.ExchangeDelete(ExchangeName(typeof(FirstTestCommand)));
            model.ExchangeDelete(ExchangeName(typeof(SecondTestCommand)));
            model.ExchangeDelete(ExchangeName(typeof(ThirdTestCommand)));
            model.ExchangeDelete(ExchangeName(typeof(FirstTestEvent)));
            model.ExchangeDelete(ExchangeName(typeof(SecondTestEvent)));
            model.ExchangeDelete(ExchangeName(typeof(ThirdTestEvent)));

            connection.Close();
        }

        private string ExchangeName(Type type) => $"{type.Namespace}:{type.Name}";

        [Test, AutoMoqData]
        public async Task Commands_are_matched_via_MessageAttribute(ThirdTestCommand testCommand, CommandReceivedAsync<AttributeTestCommand> commandReceived)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<AttributeTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Events_are_matched_via_MessageAttribute(ThirdTestEvent testEvent, EventReceivedAsync<AttributeTestEvent> eventReceived)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<AttributeTestEvent>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Commands_are_correctly_converted(ThirdTestCommand testCommand, CommandReceivedAsync<AttributeTestCommand> commandReceived)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<AttributeTestCommand>>(c => string.Equals(c.Command.Message, testCommand.Message))), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Events_are_correctly_converted(ThirdTestEvent testEvent, EventReceivedAsync<AttributeTestEvent> eventReceived)
        {
            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<AttributeTestEvent>>(e => string.Equals(e.Event.Message, testEvent.Message))), Times.Once);
        }
    }
}
