using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;

namespace Tests
{ 
    [TestFixture]
    public class NybusHostTests
    {
        [Test, AutoMoqData]
        public void BusEngine_is_required(INybusConfiguration configuration, IServiceProvider serviceProvider, ILogger<NybusHost> logger)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(null, configuration, serviceProvider, logger));
        }

        [Test, AutoMoqData]
        public void Options_is_required(IBusEngine engine, IServiceProvider serviceProvider, ILogger<NybusHost> logger)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(engine, null, serviceProvider, logger));
        }

        [Test, AutoMoqData]
        public void ServiceProvider_is_required(IBusEngine engine, INybusConfiguration configuration, ILogger<NybusHost> logger)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(engine, configuration, null, logger));
        }


        [Test, AutoMoqData]
        public void Logger_is_required(IBusEngine engine, INybusConfiguration configuration, IServiceProvider serviceProvider)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(engine, configuration, serviceProvider, null));
        }

        [Test, AutoMoqData]
        public async Task InvokeCommandAsync_forwards_message_to_engine([Frozen] IBusEngine engine, NybusHost sut, FirstTestCommand testCommand, Guid correlationId)
        {
            await sut.InvokeCommandAsync(testCommand, correlationId);

            Mock.Get(engine).Verify(p => p.SendCommandAsync(It.Is<CommandMessage<FirstTestCommand>>(m => ReferenceEquals(m.Command, testCommand) && m.Headers.CorrelationId == correlationId)), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task RaiseEventAsync_forwards_message_to_engine([Frozen] IBusEngine engine, NybusHost sut, FirstTestEvent testEvent, Guid correlationId)
        {
            await sut.RaiseEventAsync(testEvent, correlationId);

            Mock.Get(engine).Verify(p => p.SendEventAsync(It.Is<EventMessage<FirstTestEvent>>(m => ReferenceEquals(m.Event, testEvent) && m.Headers.CorrelationId == correlationId)), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task StartAsync_starts_the_engine([Frozen] IBusEngine engine, NybusHost sut)
        {
            await sut.StartAsync();

            Mock.Get(engine).Verify(p => p.StartAsync(), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task StopAsync_is_ignored_if_not_started([Frozen] IBusEngine engine, NybusHost sut)
        {
            await sut.StopAsync();

            Mock.Get(engine).Verify(p => p.StopAsync(), Times.Never);

        }

        [Test, AutoMoqData]
        public async Task StopAsync_stops_the_engine_if_started([Frozen] IBusEngine engine, NybusHost sut)
        {
            await sut.StartAsync();

            await sut.StopAsync();

            Mock.Get(engine).Verify(p => p.StopAsync(), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Handler_is_executed_when_commandMessages_are_processed([Frozen] IBusEngine engine, NybusHost sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Engine_is_notified_when_commandMessages_are_successfully_processed([Frozen] IBusEngine engine, NybusHost sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(engine).Verify(p => p.NotifySuccessAsync(testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Error_policy_is_executed_when_commandMessages_are_processed_with_errors([Frozen] IBusEngine engine, [Frozen] INybusConfiguration configuration, NybusHost sut, CommandMessage<FirstTestCommand> testMessage, Exception error)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();
            Mock.Get(receivedMessage).Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>())).Throws(error);

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(configuration.ErrorPolicy).Verify(p => p.HandleErrorAsync(engine, error, testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Null_messages_delivered_from_engine_are_discarded([Frozen] IBusEngine engine, NybusHost sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(null);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Never);
        }

        [Test, AutoMoqData]
        public async Task Handler_is_executed_when_eventMessages_are_processed([Frozen] IBusEngine engine, NybusHost sut, EventMessage<FirstTestEvent> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Engine_is_notified_when_eventMessages_are_successfully_processed([Frozen] IBusEngine engine, NybusHost sut, EventMessage<FirstTestEvent> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(engine).Verify(p => p.NotifySuccessAsync(testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Error_policy_is_executed_when_eventMessages_are_processed_with_errors([Frozen] IBusEngine engine, [Frozen] INybusConfiguration configuration, NybusHost sut, EventMessage<FirstTestEvent> testMessage, Exception error)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();
            Mock.Get(receivedMessage).Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>())).Throws(error);

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(configuration.ErrorPolicy).Verify(p => p.HandleErrorAsync(engine, error, testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Null_messages_delivered_from_engine_are_discarded([Frozen] IBusEngine engine, NybusHost sut, EventMessage<FirstTestEvent> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(engine).Setup(p => p.StartAsync()).ReturnsAsync(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(null);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Never);
        }

        [Test, AutoMoqData]
        public void ExecutionEnvironment_returns_self(NybusHost sut)
        {
            Assert.That(sut.ExecutionEnvironment, Is.SameAs(sut));
        }

        [Test, AutoMoqData]
        public async Task ExecuteCommandHandler_creates_new_scope_for_execution([Frozen] IServiceProvider serviceProvider, NybusHost sut, IDispatcher dispatcher, ICommandContext<FirstTestCommand> commandContext, IServiceScopeFactory scopeFactory, ICommandHandler<FirstTestCommand> handler)
        {
            var handlerType = handler.GetType();

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory);

            Mock.Get(serviceProvider).Setup(p => p.GetService(handlerType)).Returns(handler);

            await sut.ExecuteCommandHandlerAsync(dispatcher, commandContext, handlerType);

            Mock.Get(scopeFactory).Verify(p => p.CreateScope(), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task ExecuteCommandHandler_executes_handler([Frozen] IServiceProvider serviceProvider, NybusHost sut, IDispatcher dispatcher, ICommandContext<FirstTestCommand> commandContext, IServiceScopeFactory scopeFactory, ICommandHandler<FirstTestCommand> handler)
        {
            var handlerType = handler.GetType();

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory);

            Mock.Get(serviceProvider).Setup(p => p.GetService(handlerType)).Returns(handler);

            await sut.ExecuteCommandHandlerAsync(dispatcher, commandContext, handlerType);

            Mock.Get(handler).Verify(p => p.HandleAsync(dispatcher, commandContext), Times.Once);
        }

        [Test, AutoMoqData]
        public void ExecuteCommandHandler_throws_if_handler_is_not_registered([Frozen] IServiceProvider serviceProvider, NybusHost sut, IDispatcher dispatcher, ICommandContext<FirstTestCommand> commandContext, IServiceScopeFactory scopeFactory)
        {
            var handlerType = typeof(FirstTestCommandHandler);

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory);

            Mock.Get(serviceProvider).Setup(p => p.GetService(handlerType)).Throws<InvalidOperationException>();

            Assert.ThrowsAsync<ConfigurationException>(() => sut.ExecuteCommandHandler(dispatcher, commandContext, handlerType));
        }

        [Test, AutoMoqData]
        public async Task ExecuteEventHandler_creates_new_scope_for_execution([Frozen] IServiceProvider serviceProvider, NybusHost sut, IDispatcher dispatcher, IEventContext<FirstTestEvent> eventContext, IServiceScopeFactory scopeFactory, IEventHandler<FirstTestEvent> handler)
        {
            var handlerType = handler.GetType();

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory);

            Mock.Get(serviceProvider).Setup(p => p.GetService(handlerType)).Returns(handler);

            await sut.ExecuteEventHandlerAsync(dispatcher, eventContext, handlerType);

            Mock.Get(scopeFactory).Verify(p => p.CreateScope(), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task ExecuteEventHandler_executes_handler([Frozen] IServiceProvider serviceProvider, NybusHost sut, IDispatcher dispatcher, IEventContext<FirstTestEvent> eventContext, IServiceScopeFactory scopeFactory, IEventHandler<FirstTestEvent> handler)
        {
            var handlerType = handler.GetType();

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory);

            Mock.Get(serviceProvider).Setup(p => p.GetService(handlerType)).Returns(handler);

            await sut.ExecuteEventHandlerAsync(dispatcher, eventContext, handlerType);

            Mock.Get(handler).Verify(p => p.HandleAsync(dispatcher, eventContext), Times.Once);
        }

        [Test, AutoMoqData]
        public void ExecuteEventHandler_throws_if_handler_is_not_registered([Frozen] IServiceProvider serviceProvider, NybusHost sut, IDispatcher dispatcher, IEventContext<FirstTestEvent> eventContext, IServiceScopeFactory scopeFactory)
        {
            var handlerType = typeof(FirstTestEventHandler);

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory);

            Mock.Get(serviceProvider).Setup(p => p.GetService(handlerType)).Throws<InvalidOperationException>();

            Assert.ThrowsAsync<ConfigurationException>(() => sut.ExecuteEventHandler(dispatcher, eventContext, handlerType));
        }
    }
}