using System.Collections.Generic;
using System.Threading.Tasks;
using FakeRabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class AutomaticHandlerConfigurationSetupTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FakeServer server, SecondTestCommand testCommand, CommandReceivedAsync<SecondTestCommand> commandReceived)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.UseConfiguration(configuration);

                nybus.SubscribeToCommand<SecondTestCommand>();
            },
            services =>
            {
                services.AddSingleton(commandReceived);
                services.AddSingleton<ICommandHandler<SecondTestCommand>, SecondTestCommandHandler>();
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<SecondTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FakeServer server, SecondTestEvent testEvent, EventReceivedAsync<SecondTestEvent> eventReceived)
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:ErrorPolicy:ProviderName"] = "retry",
                ["Nybus:ErrorPolicy:MaxRetries"] = "5",
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration = configurationBuilder.Build();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
                });

                nybus.UseConfiguration(configuration);

                nybus.SubscribeToEvent<SecondTestEvent>();
            },
            services =>
            {
                services.AddSingleton(eventReceived);
                services.AddSingleton<IEventHandler<SecondTestEvent>, SecondTestEventHandler>();

            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<SecondTestEvent>>()));
        }

    }
}