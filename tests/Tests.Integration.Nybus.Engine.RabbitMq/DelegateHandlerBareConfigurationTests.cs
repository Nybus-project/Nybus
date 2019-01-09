using System.Text;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using RabbitMQ.Client;

namespace Tests
{
    [TestFixture]
    public class DelegateHandlerBareConfigurationTests
    {
        [Test, AutoMoqData]
        public async Task DoSomething(ServiceCollection services, [Frozen] IModel channel, IConnectionFactory connectionFactory, FirstTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceived<FirstTestCommand>>();

            services.AddLogging(b => b.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration =>
                    {
                        configuration.ConnectionFactory = connectionFactory;
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
        }
    }
}
