using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nybus.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> items) where T : class => items.Where(i => i != null);

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> items) => items ?? Array.Empty<T>();

        public static IEnumerable<T> IfNull<T>(this IEnumerable<T> items, IEnumerable<T> alternative) => items ?? alternative;
    }
}
