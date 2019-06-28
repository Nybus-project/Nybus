using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeRabbitMQ;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Utils;
using static Tests.TestUtils;

namespace Tests
{
    [TestFixture]
    public class IssueTests
    {
        [Test, AutoMoqData]
        [Description("https://github.com/Nybus-project/Nybus/issues/90")]
        public async Task Issue90(FakeServer server, CommandReceivedAsync<FirstTestCommand> commandReceived, Exception exception, FirstTestCommand testCommand)
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

            var host = CreateNybusHost(nybus =>
            {
                nybus.SubscribeToCommand(commandReceived);

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c => c.ConnectionFactory = server.CreateConnectionFactory());
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