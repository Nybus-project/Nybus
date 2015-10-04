using Moq;
using NLog;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;

namespace Tests.Logging
{
    [TestFixture]
    public class NLogLoggerProviderTests
    {
        private IFixture _fixture;
        private LogFactory _logFactory;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();

            _logFactory= new LogFactory();
        }

        [Test, ExpectedException]
        public void LogFactory_is_required()
        {
            new NLogLoggerProvider(null);
        }

        private NLogLoggerProvider CreateSystemUnderTests()
        {
            return new NLogLoggerProvider(_logFactory);
        }

        [Test]
        public void A_test()
        {
            var sut = CreateSystemUnderTests();

            var logger = sut.CreateLogger(_fixture.Create<string>());

            Assert.That(logger, Is.InstanceOf<NLogLogger>());
        }

        [Test]
        public void Dispose_disposed_the_factory()
        {
            var sut = CreateSystemUnderTests();

            sut.Dispose();
        }
    }
}