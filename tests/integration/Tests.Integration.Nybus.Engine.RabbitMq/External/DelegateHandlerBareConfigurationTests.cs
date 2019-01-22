using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class DelegateHandlerBareConfigurationTests
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

        private static IBusHost CreateNybusHost(Action<INybusConfigurator> configurator)
        {
            var services = new ServiceCollection().AddLogging();

            services.AddNybus(configurator);

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            return host;
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_commands(FirstTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceivedAsync<FirstTestCommand>>();

            var host = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = new ConnectionFactory());
                });

                nybus.SubscribeToCommand(commandReceived);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Host_can_loopback_events(FirstTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceivedAsync<FirstTestEvent>>();

            var host = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = new ConnectionFactory());
                });

                nybus.SubscribeToEvent(eventReceived);
            });

            await host.StartAsync();

            await host.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()));
        }

        [Test, AutoMoqData]
        public async Task Hosts_can_exchange_commands(FirstTestCommand testCommand)
        {
            var sender = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = new ConnectionFactory());
                });
            });

            var commandReceived = Mock.Of<CommandReceivedAsync<FirstTestCommand>>();

            var receiver = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = new ConnectionFactory());
                });

                nybus.SubscribeToCommand(commandReceived);
            });

            await sender.StartAsync();

            await receiver.StartAsync();

            await sender.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await receiver.StopAsync();

            await sender.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()));
        }

        [Test, AutoMoqData]
        public async Task Hosts_can_exchange_events(FirstTestEvent testEvent)
        {
            var sender = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = new ConnectionFactory());
                });
            });

            var eventReceived = Mock.Of<EventReceivedAsync<FirstTestEvent>>();

            var receiver = CreateNybusHost(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration => configuration.ConnectionFactory = new ConnectionFactory());
                });

                nybus.SubscribeToEvent(eventReceived);
            });

            await sender.StartAsync();

            await receiver.StartAsync();

            await sender.Bus.RaiseEventAsync(testEvent);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await receiver.StopAsync();

            await sender.StopAsync();

            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()));
        }
    }
}
