using System;
using System.Collections.Generic;
using AutoFixture;
using Moq;

namespace Tests
{
    public static class ObservableTestExtensions
    {
        public static IReadOnlyList<T> DumpInList<T>(this IObservable<T> sequence)
        {
            var incomingItems = new List<T>();

            sequence.Subscribe(incomingItems.Add);

            return incomingItems;
        }
    }

    public static class ObserverTestExtensions
    {
        public static void OnNext<T>(this IObserver<T> observer, Generator<T> generator)
        {
            using (var enumerator = generator.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    observer.OnNext(enumerator.Current);
                }
            }
        }

        public static void ReceivedItems<T>(this IObserver<T> observer, int times)
        {
            Mock.Get(observer).Verify(p => p.OnNext(It.IsAny<T>()), Times.Exactly(times));
        }

        public static void ReceivedNoItem<T>(this IObserver<T> observer)
        {
            Mock.Get(observer).Verify(p => p.OnNext(It.IsAny<T>()), Times.Never);
        }

        public static void HasError<T>(this IObserver<T> observer, Exception error, int times = 1)
        {
            Mock.Get(observer).Verify(p => p.OnError(error), Times.Exactly(times));
        }

        public static void IsCompleted<T>(this IObserver<T> observer, int times = 1)
        {
            Mock.Get(observer).Verify(p => p.OnCompleted(), Times.Exactly(times));
        }
    }
}