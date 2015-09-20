using System.Threading.Tasks;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class DelegateEventHandlerTests
    {
        private IFixture fixture;
        private int invocations;

        [SetUp]
        public void Initialize()
        {
            invocations = 0;
            fixture = new Fixture();
        }
        private DelegateEventHandler<TestEvent> CreateSystemUnderTest()
        {
            return new DelegateEventHandler<TestEvent>(TestDelegate);
        }

        [Test]
        [ExpectedException]
        public void Handler_is_required()
        {
            new DelegateEventHandler<TestEvent>(null);
        }

        [Test]
        public async Task Delegate_is_invoked()
        {
            var sut = CreateSystemUnderTest();

            var context = fixture.Create<EventContext<TestEvent>>();

            await sut.Handle(context);

            Assert.That(invocations, Is.GreaterThan(0));
        }

        private Task TestDelegate(EventContext<TestEvent> context)
        {
            invocations++;
            return Task.CompletedTask;
        }

    }
}