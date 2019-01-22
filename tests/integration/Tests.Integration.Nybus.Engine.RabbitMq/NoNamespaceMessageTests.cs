using System;
using System.Threading.Tasks;
using FakeRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using static Tests.TestUtils;

namespace Tests
{
    [TestFixture]
    public class NoNamespaceMessageTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, NoNamespaceCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceivedAsync<NoNamespaceCommand>>();

            var host = CreateNybusHost(nybus =>
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

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<NoNamespaceCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, NoNamespaceEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceivedAsync<NoNamespaceEvent>>();

            var host = CreateNybusHost(nybus =>
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

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<NoNamespaceEvent>>()));
        }

        [Test, AutoMoqData]
        public async Task Hosts_can_exchange_commands(FakeServer server, NoNamespaceCommand testCommand)
        {
            var sender = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            var commandReceived = Mock.Of<CommandReceivedAsync<NoNamespaceCommand>>();

            var receiver = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.SubscribeToCommand(commandReceived);
            });

            await sender.StartAsync();

            await receiver.StartAsync();

            await sender.Bus.InvokeCommandAsync(testCommand);

            await receiver.StopAsync();

            await sender.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<NoNamespaceCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Hosts_can_exchange_events(FakeServer server, NoNamespaceEvent testEvent)
        {
            var sender = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });
            });

            var eventReceived = Mock.Of<EventReceivedAsync<NoNamespaceEvent>>();

            var receiver = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.SubscribeToEvent(eventReceived);
            });

            await sender.StartAsync();

            await receiver.StartAsync();

            await sender.Bus.RaiseEventAsync(testEvent);

            await receiver.StopAsync();

            await sender.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<NoNamespaceEvent>>()));
        }
    }
}
