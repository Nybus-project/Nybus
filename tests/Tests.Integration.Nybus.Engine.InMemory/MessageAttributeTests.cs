using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Samples;

namespace Tests
{
    [TestFixture]
    public class MessageAttributeTests
    {
        [Test, AutoMoqData]
        public async Task Commands_are_matched_via_MessageAttribute(ServiceCollection services, ThirdTestCommand testCommand)
        {
            var commandReceived = Mock.Of<CommandReceived<AttributeTestCommand>>();

            services.AddLogging(l => l.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToCommand(commandReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.InvokeCommandAsync(testCommand);

            await host.StopAsync();

            //Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<ICommandContext<AttributeTestCommand>>(c => string.Equals(c.Command.Message, testCommand.Message))), Times.Once);
            Mock.Get(commandReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<AttributeTestCommand>>()), Times.Once);

        }

        [Test, AutoMoqData]
        public async Task Events_are_matched_via_MessageAttribute(ServiceCollection services, ThirdTestEvent testEvent)
        {
            var eventReceived = Mock.Of<EventReceived<AttributeTestEvent>>();

            services.AddLogging(l => l.AddDebug());

            services.AddNybus(nybus =>
            {
                nybus.UseInMemoryBusEngine();

                nybus.SubscribeToEvent(eventReceived);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            var bus = serviceProvider.GetRequiredService<IBus>();

            await host.StartAsync();

            await bus.RaiseEventAsync(testEvent);

            await host.StopAsync();

            //Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.Is<IEventContext<AttributeTestEvent>>(e => string.Equals(e.Event.Message, testEvent.Message))), Times.Once);
            Mock.Get(eventReceived).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<AttributeTestEvent>>()), Times.Once);
        }
    }
}
