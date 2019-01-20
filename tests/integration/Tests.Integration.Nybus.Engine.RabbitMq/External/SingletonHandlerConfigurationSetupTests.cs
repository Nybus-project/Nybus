using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class SingletonHandlerConfigurationSetupTests
    {
        [TearDown]
        public void OnTestComplete()
        {
            var connectionFactory = new ConnectionFactory();
            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();

            model.ExchangeDelete(typeof(FirstTestCommand).FullName);
            model.ExchangeDelete(typeof(FirstTestEvent).FullName);

            connection.Close();
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FirstTestCommand testCommand)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var handler = Mock.Of<FirstTestCommandHandler>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(handler);

                nybus.UseConfiguration(configuration);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(handler).Verify(p => p.HandleAsync(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FirstTestEvent testEvent)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var handler = Mock.Of<FirstTestEventHandler>();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(handler);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });

                nybus.UseConfiguration(configuration);
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(handler).Verify(p => p.HandleAsync(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }

    }
}