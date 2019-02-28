using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Moq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class NybusFunctionTests
    {
        [Test, AutoMoqData]
        public void TestFunction_is_set_up(TestFunction sut)
        {
            Assert.That(sut.BusHost, Is.Not.Null);

            Assert.That(sut.Handler, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void TestFunctionWithHandler_is_set_up(TestFunctionWithHandler sut)
        {
            Assert.That(sut.BusHost, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void TestFunctionWithNoHandler_is_set_up(TestFunctionWithNoHandler sut)
        {
            Assert.That(sut.BusHost, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public async Task FunctionHandlerAsync_starts_bus(TestFunction sut, string payload, ILambdaContext context)
        {
            await sut.FunctionHandlerAsync(payload, context);

            Mock.Get(sut.BusHost).Verify(p => p.StartAsync());
        }

        [Test, AutoMoqData]
        public async Task FunctionHandlerAsync_stops_bus(TestFunction sut, string payload, ILambdaContext context)
        {
            await sut.FunctionHandlerAsync(payload, context);

            Mock.Get(sut.BusHost).Verify(p => p.StopAsync());
        }

        [Test, AutoMoqData]
        public async Task FunctionHandlerAsync_starts_bus(TestFunctionWithHandler sut, string payload, ILambdaContext context)
        {
            await sut.FunctionHandlerAsync(payload, context);

            Mock.Get(sut.BusHost).Verify(p => p.StartAsync());
        }

        [Test, AutoMoqData]
        public async Task FunctionHandlerAsync_stops_bus(TestFunctionWithHandler sut, string payload, ILambdaContext context)
        {
            await sut.FunctionHandlerAsync(payload, context);

            Mock.Get(sut.BusHost).Verify(p => p.StopAsync());
        }

        [Test, AutoMoqData]
        public void FunctionHandlerAsync_starts_bus(TestFunctionWithNoHandler sut, string payload, ILambdaContext context)
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => sut.FunctionHandlerAsync(payload, context));

            Mock.Get(sut.BusHost).Verify(p => p.StartAsync());
        }

        [Test, AutoMoqData]
        public void FunctionHandlerAsync_stops_bus(TestFunctionWithNoHandler sut, string payload, ILambdaContext context)
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => sut.FunctionHandlerAsync(payload, context));

            Mock.Get(sut.BusHost).Verify(p => p.StopAsync());
        }

        [Test, AutoMoqData]
        public async Task FunctionHandlerAsync_invokes_handler(TestFunction sut, string payload, ILambdaContext context)
        {
            await sut.FunctionHandlerAsync(payload, context);

            Mock.Get(sut.Handler).Verify(p => p.HandleAsync(payload, context));
        }

        [Test, AutoMoqData]
        public async Task FunctionHandlerAsync_invokes_registered_handler(TestFunctionWithHandler sut, string payload, ILambdaContext context)
        {
            await sut.FunctionHandlerAsync(payload, context);

            Mock.Get(TestHandler.InnerHandler).Verify(p => p.HandleAsync(payload, context));
        }

        [Test, AutoMoqData]
        public void FunctionHandlerAsync_throws_if_no_handler_is_registered(TestFunctionWithNoHandler sut, string payload, ILambdaContext context)
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => sut.FunctionHandlerAsync(payload, context));
        }
    }
}