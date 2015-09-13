using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class DelegateCommandHandlerTests
    {
        private IFixture fixture;
        private int invocations;

        [SetUp]
        public void Initialize()
        {
            invocations = 0;
            fixture = new Fixture();
        }
        private DelegateCommandHandler<TestCommand> CreateSystemUnderTest()
        {
            return new DelegateCommandHandler<TestCommand>(TestDelegate);
        }

        [Test][ExpectedException]
        public void Handler_is_required()
        {
            new DelegateCommandHandler<TestCommand>(null);
        }

        [Test]
        public async Task Delegate_is_invoked()
        {
            var sut = CreateSystemUnderTest();

            var context = fixture.Create<CommandContext<TestCommand>>();

            await sut.Handle(context);

            Assert.That(invocations, Is.GreaterThan(0));
        }

        private Task TestDelegate(CommandContext<TestCommand> context)
        {
            invocations++;
            return Task.CompletedTask;
        }
    }
}