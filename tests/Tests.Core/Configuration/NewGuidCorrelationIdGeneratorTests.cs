using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class NewGuidCorrelationIdGeneratorTests
    {
        private NewGuidCorrelationIdGenerator CreateSystemUnderTest()
        {
            return new NewGuidCorrelationIdGenerator();
        }

        [Test]
        public void Can_generate_a_new_correlationId()
        {
            var sut = CreateSystemUnderTest();

            sut.Generate();
        }
    }
}
