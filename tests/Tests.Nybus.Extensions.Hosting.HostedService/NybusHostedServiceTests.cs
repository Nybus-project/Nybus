using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class NybusHostedServiceTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(NybusHostedService).GetConstructors());
        }

        [Test, AutoMoqData]
        public async Task StartAsync_starts_host([Frozen] IBusHost host, NybusHostedService sut, CancellationToken cancellationToken)
        {
            await sut.StartAsync(cancellationToken);

            Mock.Get(host).Verify(p => p.StartAsync());
        }

        [Test, AutoMoqData]
        public async Task StopAsync_stops_host([Frozen] IBusHost host, NybusHostedService sut, CancellationToken cancellationToken)
        {
            await sut.StopAsync(cancellationToken);

            Mock.Get(host).Verify(p => p.StopAsync());
        }
    }
}