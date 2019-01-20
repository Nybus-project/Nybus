using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class RegisteredHandlerConfigurationSetupTests
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
        public async Task Host_can_loopback_commands(SecondTestCommand testCommand)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var commandReceived = Mock.Of<CommandReceived<SecondTestCommand>>();
            var mockHandler = new Mock<SecondTestCommandHandler>(commandReceived);
            var handler = mockHandler.Object;

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });

                nybus.UseConfiguration(configuration);

                nybus.SubscribeToCommand<SecondTestCommand, SecondTestCommandHandler>();
            },
            services =>
            {
                services.AddSingleton(commandReceived);
                services.AddSingleton(handler);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<SecondTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(SecondTestEvent testEvent)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var eventReceived = Mock.Of<EventReceived<SecondTestEvent>>();
            var mockHandler = new Mock<SecondTestEventHandler>(eventReceived);
            var handler = mockHandler.Object;

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });

                nybus.UseConfiguration(configuration);

                nybus.SubscribeToEvent<SecondTestEvent, SecondTestEventHandler>();
            },
            services =>
            {
                services.AddSingleton(eventReceived);
                services.AddSingleton(handler);
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<SecondTestEvent>>()));

        }

    }
}