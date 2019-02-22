using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.Configuration
{
    [TestFixture]
    public class ExchangeManagerTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(ExchangeManager).GetConstructors());
        }

        [Test, AutoMoqData]
        public void EnsureExchangeExists_forwards_settings([Frozen] ExchangeOptions options, ExchangeManager sut, IModel model, string exchangeName, string exchangeType)
        {
            sut.EnsureExchangeExists(model, exchangeName, exchangeType);

            Mock.Get(model).Verify(p => p.ExchangeDeclare(exchangeName, exchangeType, options.IsDurable, options.IsAutoDelete, options.Properties));
        }
    }
}