using System.Threading.Tasks;
using FakeRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class DelegateHandlerBareConfigurationTests
    {
        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(ServiceCollection services, FakeServer server, FirstTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceived<FirstTestCommand>>();

            services.AddLogging(b => b.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration =>
                    {
                        configuration.ConnectionFactory = server.CreateConnectionFactory();
                    });
                });

                nybus.SubscribeToCommand(commandReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            await host.StartAsync();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(ServiceCollection services, FakeServer server, FirstTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceived<FirstTestEvent>>();

            services.AddLogging(b => b.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration =>
                    {
                        configuration.ConnectionFactory = server.CreateConnectionFactory();
                    });
                });

                nybus.SubscribeToEvent(eventReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            await host.StartAsync();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()));
        }
    }
}
