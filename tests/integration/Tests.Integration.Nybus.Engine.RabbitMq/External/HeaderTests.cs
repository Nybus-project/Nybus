using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class HeaderTests
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
        public async Task Host_can_loopback_commands(SecondTestCommand testCommand, string headerKey, string headerValue)
        {
            var commandReceived = Mock.Of<CommandReceivedAsync<SecondTestCommand>>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
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

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<SecondTestCommand>>(c => c.Message.Headers.ContainsKey(headerKey) && c.Message.Headers[headerKey] == headerValue)));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(SecondTestEvent testEvent, string headerKey, string headerValue)
        {
            var eventReceived = Mock.Of<EventReceivedAsync<SecondTestEvent>>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent(eventReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
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

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<SecondTestEvent>>(c => c.Message.Headers.ContainsKey(headerKey) && c.Message.Headers[headerKey] == headerValue)));
        }

    }
}