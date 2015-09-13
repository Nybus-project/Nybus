using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;

namespace Tests.Logging
{
    [TestFixture]
    public class NopLoggerTests
    {
        private IFixture fixture = new Fixture();

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        private NopLogger CreateSystemUnderTests()
        {
            return new NopLogger();
        }

        [Test]
        public async Task LogAsync_wont_throw()
        {
            var sut = CreateSystemUnderTests();

            var level = fixture.Create<LogLevel>();
            var message = fixture.Create<string>();
            var data = new
            {
                text = fixture.Create<string>(),
                value = fixture.Create<int>(),
                time = fixture.Create<DateTimeOffset>(),
                flag = fixture.Create<bool>()
            };


            await sut.LogAsync(level, message, data);
        }

        [Test]
        public void Log_wont_throw()
        {
            var sut = CreateSystemUnderTests();

            var level = fixture.Create<LogLevel>();
            var message = fixture.Create<string>();
            var data = new
            {
                text = fixture.Create<string>(),
                value = fixture.Create<int>(),
                time = fixture.Create<DateTimeOffset>(),
                flag = fixture.Create<bool>()
            };


            sut.Log(level, message, data);
        }
    }
}
