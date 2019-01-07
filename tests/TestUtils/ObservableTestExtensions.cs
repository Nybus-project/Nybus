using System;
using System.Collections.Generic;

namespace Tests {
    public static class ObservableTestExtensions
    {
        public static IReadOnlyList<T> DumpInList<T>(this IObservable<T> sequence)
        {
            var incomingItems = new List<T>();

            sequence.Subscribe(incomingItems.Add);

            return incomingItems;
        }
    }
}