using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Nybus.Utils;
using Ploeh.AutoFixture;

namespace Tests.Utils
{
    [TestFixture]
    public class TaskExtensionsTests
    {
        [TestFixture]
        public class WaitAndUnwrapException
        {
            private IFixture fixture;

            [SetUp]
            public void Initialize()
            {
                fixture = new Fixture();
            }

            [Test]
            [ExpectedException(typeof(OperationCanceledException))]
            public void Throws_when_operation_is_canceled()
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                cancellationTokenSource.Cancel();

                Never<int>().WaitAndUnwrapException(cancellationTokenSource.Token);
            }

            [Test]
            public void Returns_when_operation_is_completed()
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                Task.CompletedTask.WaitAndUnwrapException(cancellationTokenSource.Token);
            }

            [Test]
            public void Returns_when_value_is_ready_with_token()
            {
                var testValue = fixture.Create<int>();

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                var actualValue = Value(testValue).WaitAndUnwrapException(cancellationTokenSource.Token);

                Assert.That(actualValue, Is.EqualTo(testValue));
            }

            [Test]
            public void Returns_when_value_is_ready()
            {
                var testValue = fixture.Create<int>();

                var actualValue = Value(testValue).WaitAndUnwrapException();

                Assert.That(actualValue, Is.EqualTo(testValue));
            }


            [Test]
            [ExpectedException(typeof (NotImplementedException))]
            public void Unwraps_exception_when_thrown()
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                Task.Run(() => { throw new NotImplementedException(); }).WaitAndUnwrapException(cancellationTokenSource.Token);
            }

            [Test]
            [ExpectedException(typeof(NotImplementedException))]
            public void Unwraps_exception_when_thrown_typed()
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                Task.Run(() => FaultyMethod<int>()).WaitAndUnwrapException(cancellationTokenSource.Token);
            }

            private T FaultyMethod<T>()
            {
                throw new NotImplementedException();
            }
        }

        private static Task<T> Never<T>() => new TaskCompletionSource<T>().Task;

        private static Task Never() => new TaskCompletionSource<int>().Task;

        private static Task<T> DefaultValue<T>() => Task.FromResult(default(T));

        private static Task<T> Value<T>(T value) => Task.FromResult(value);
    }
}
