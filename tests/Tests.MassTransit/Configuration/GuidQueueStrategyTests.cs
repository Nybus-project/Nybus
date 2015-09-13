using System;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class GuidQueueStrategyTests
    {
        [Test]
        public void GetQueueName_returns_stringified_Guid()
        {
            var sut = new GuidQueueStrategy();

            var value = sut.GetQueueName();

            Guid.Parse(value);
        }
    }
}