using Moq;
using NUnit.Framework;
using Nybus;
using Topshelf;

namespace Tests
{
    [TestFixture]
    public class StartupTests
    {
        [Test, AutoMoqData]
        public void OnStart_starts_the_host(TestStartup sut, IBusHost busHost, HostControl control)
        {
            sut.OnStart(busHost, control);

            Mock.Get(busHost).Verify(p => p.StartAsync(), Times.Once);
        }

        [Test, AutoMoqData]
        public void OnStop_stops_the_host(TestStartup sut, IBusHost busHost, HostControl control)
        {
            sut.OnStop(busHost, control);

            Mock.Get(busHost).Verify(p => p.StopAsync(), Times.Once);
        }
    }
}