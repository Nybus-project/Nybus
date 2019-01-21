using System.Collections.Generic;
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
    public class HeaderTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, SecondTestCommand testCommand, string headerKey, string headerValue)
        {
            var commandReceived = Mock.Of<CommandReceived<SecondTestCommand>>();

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            },
            services =>
            {
                services.AddSingleton(commandReceived);
                services.AddSingleton<ICommandHandler<SecondTestCommand>, SecondTestCommandHandler>();
            });

            await host.StartAsync();

            var headers = new Dictionary<string, string>
            {
                [headerKey] = headerValue
            };

            await host.Bus.InvokeCommandAsync(testCommand, headers);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<SecondTestCommand>>(c => c.Message.Headers.ContainsKey(headerKey) && c.Message.Headers[headerKey] == headerValue)));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, SecondTestEvent testEvent, string headerKey, string headerValue)
        {
            var eventReceived = Mock.Of<EventReceived<SecondTestEvent>>();

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });
            },
            services =>
            {
                services.AddSingleton(eventReceived);
                services.AddSingleton<IEventHandler<SecondTestEvent>, SecondTestEventHandler>();
            });

            await host.StartAsync();

            var headers = new Dictionary<string, string>
            {
                [headerKey] = headerValue
            };

            await host.Bus.RaiseEventAsync(testEvent, headers);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<SecondTestEvent>>(c => c.Message.Headers.ContainsKey(headerKey) && c.Message.Headers[headerKey] == headerValue)));
        }

    }
}