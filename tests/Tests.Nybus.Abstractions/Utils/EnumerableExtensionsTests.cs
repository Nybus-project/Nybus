using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test, AutoMoqData]
        public void NotNull_filters_null_items_out(IEnumerable<string> items)
        {
            items = items.Concat(new string[] { null });

            Assume.That(items.All(i => i != null), Is.False);

            Assert.That(EnumerableExtensions.NotNull(items).All(i => i != null), Is.True);
        }

        [Test, AutoMoqData]
        public void EmptyIfNull_returns_same_set_if_not_null(IEnumerable<string> items)
        {
            Assume.That(items, Is.Not.Null);

            Assert.That(items.EmptyIfNull(), Is.SameAs(items));
        }

        [Test, AutoMoqData]
        public void EmptyIfNull_returns_empty_set_if_null()
        {
            IEnumerable<string> items = null;

            Assume.That(items, Is.Null);

            Assert.That(items.EmptyIfNull(), Is.Not.Null);
            Assert.That(items.EmptyIfNull(), Is.Empty);
        }

        [Test, AutoMoqData]
        public void IfNull_returns_set_if_not_null(IEnumerable<string> items, IEnumerable<string> alternative)
        {
            Assert.That(items.IfNull(alternative), Is.SameAs(items));
        }

        [Test, AutoMoqData]
        public void IfNull_returns_alternative_if_not_null(IEnumerable<string> alternative)
        {
            IEnumerable<string> items = null;

            Assert.That(items.IfNull(alternative), Is.SameAs(alternative));
        }
    }
}
