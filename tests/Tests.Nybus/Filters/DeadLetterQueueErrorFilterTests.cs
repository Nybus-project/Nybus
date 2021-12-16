using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;

namespace Tests.Filters
{
    [TestFixture]
    public class DeadLetterQueueErrorFilterTests
    {
        [Test, CustomAutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DeadLetterQueueErrorFilter).GetConstructors());
        }
        
        [TestFixture]
        public class DeadLetterQueueErrorFilterTestsCommands : DeadLetterQueueErrorFilterTestsBase<ICommandContext<FirstTestCommand>, CommandErrorDelegate<FirstTestCommand>>
        {
            protected override Task HandleErrorAsync(DeadLetterQueueErrorFilter sut, ICommandContext<FirstTestCommand> context, Exception exception, CommandErrorDelegate<FirstTestCommand> errorDelegate)
            {
                return sut.HandleErrorAsync(context, exception, errorDelegate);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_sends_to_next_on_exception(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                ICommandContext<FirstTestCommand> context,
                CommandErrorDelegate<FirstTestCommand> errorDelegate,
                TestException exception,
                TestException engineException)
            {
                Mock.Get(engine).Setup(e => e.SendMessageToErrorQueueAsync(context.Message)).Throws(engineException);

                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(errorDelegate).Verify(d => d(context, exception));
            }
        }

        [TestFixture]
        public class DeadLetterQueueErrorFilterTestsEvents : DeadLetterQueueErrorFilterTestsBase<IEventContext<FirstTestEvent>, EventErrorDelegate<FirstTestEvent>>
        {
            protected override Task HandleErrorAsync(DeadLetterQueueErrorFilter sut, IEventContext<FirstTestEvent> context, Exception exception,
                EventErrorDelegate<FirstTestEvent> errorDelegate)
            {
                return sut.HandleErrorAsync(context, exception, errorDelegate);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_sends_to_next_on_exception(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                IEventContext<FirstTestEvent> context,
                EventErrorDelegate<FirstTestEvent> errorDelegate,
                TestException exception,
                TestException engineException)
            {
                Mock.Get(engine).Setup(e => e.SendMessageToErrorQueueAsync(context.Message)).Throws(engineException);

                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(errorDelegate).Verify(d => d(context, exception));
            }
        }

        public abstract class DeadLetterQueueErrorFilterTestsBase<TContext, TErrorDelegate> where TContext : IContext
        {
            protected abstract Task HandleErrorAsync(DeadLetterQueueErrorFilter sut, TContext context, Exception exception, TErrorDelegate errorDelegate);

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_notifies_succcess(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine).Verify(e => e.NotifySuccessAsync(context.Message), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_sends_message_to_error_queue(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine).Verify(e => e.SendMessageToErrorQueueAsync(context.Message), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_adds_exception_message_header(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            m.Headers["DLQ-Fault-Message"].Equals(exception.Message))), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_does_not_add_exception_message_header_if_exception_is_null(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate)
            {
                await HandleErrorAsync(sut, context, null, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            !m.Headers.ContainsKey("DLQ-Fault-Message"))), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_adds_exception_stackTrace_header(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            m.Headers["DLQ-Fault-StackTrace"].Equals(exception.StackTrace))), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_does_not_add_exception_stackTrace_header_if_exception_is_null(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate)
            {
                await HandleErrorAsync(sut, context, null, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            !m.Headers.ContainsKey("DLQ-Fault-StackTrace"))), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_adds_machineName_header(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            m.Headers["DLQ-Error-Host"].Equals(Environment.MachineName))), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_adds_current_process_name_header(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            m.Headers["DLQ-Error-Process"].Equals(Process.GetCurrentProcess().ProcessName))), Times.Once);
            }

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_adds_assemblyName_header_if_has_EntryAssembly(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                SetEntryAssembly(Assembly.GetCallingAssembly());

                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            m.Headers["DLQ-Error-Assembly"].Equals(Assembly.GetEntryAssembly().GetName().Name))), Times.Once);

                SetEntryAssembly(null);
            }

            private void SetEntryAssembly(Assembly assembly)
            {
#if NET472
                AppDomainManager manager = new AppDomainManager();
                FieldInfo entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
                entryAssemblyfield.SetValue(manager, assembly);

                AppDomain domain = AppDomain.CurrentDomain;
                FieldInfo domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
                domainManagerField.SetValue(domain, manager);
#endif
            }

#if NET472
            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_does_not_add_assemblyName_header_if_has_not_EntryAssembly(
                [Frozen] IBusEngine engine,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception)
            {
                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(engine)
                    .Verify(
                        e => e.SendMessageToErrorQueueAsync(It.Is<Message>(m =>
                            !m.Headers.ContainsKey("DLQ-Error-Assembly"))), Times.Once);
            }
#endif

            [Test, CustomAutoMoqData]
            public async Task HandleErrorAsync_logs_error_on_exception(
                [Frozen] IBusEngine engine,
                [Frozen] ILogger<DeadLetterQueueErrorFilter> logger,
                DeadLetterQueueErrorFilter sut,
                TContext context,
                TErrorDelegate errorDelegate,
                TestException exception,
                TestException engineException)
            {
                Mock.Get(engine).Setup(e => e.SendMessageToErrorQueueAsync(context.Message)).Throws(engineException);

                Mock.Get(logger).Setup(
                    l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>())).Verifiable();

                await HandleErrorAsync(sut, context, exception, errorDelegate);

                Mock.Get(logger).Verify(
                    l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }
        }
    }
}