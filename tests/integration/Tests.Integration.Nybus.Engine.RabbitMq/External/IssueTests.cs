using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;
using Nybus.Utils;
using RabbitMQ.Client;

namespace Tests.External
{
    [ExternalTestFixture]
    public class IssueTests
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
        [Description("https://github.com/Nybus-project/Nybus/issues/90")]
        public async Task Issue90(CommandReceivedAsync<FirstTestCommand> commandReceived, Exception exception, FirstTestCommand testCommand)
        {
            const int maxRetries = 5;

            Mock.Get(commandReceived)
                .Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()))
                .Throws(exception);
                
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Nybus:CommandErrorFilters:0:type"] = "retry",
                ["Nybus:CommandErrorFilters:0:maxRetries"] = maxRetries.Stringfy()
            });

            var configuration = builder.Build();

            var host = TestUtils.CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = new ConnectionFactory());
                });

                nybus.UseConfiguration(configuration);
            });

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(testCommand);

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            await host.StopAsync();

            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Exactly(maxRetries));
        }
    }
}
