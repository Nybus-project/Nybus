using AutoFixture.NUnit3;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class ConcurrentListTests
    {
        [Test, AutoData]
        public void IsEmpty_returns_true_if_no_item(ConcurrentList<string> sut)
        {
            Assert.That(sut.IsEmpty, Is.True);
        }

        [Test, AutoData]
        public void Add_adds_item(ConcurrentList<string> sut, string item)
        {
            sut.Add(item);

            Assert.That(sut.Contains(item), Is.True);
        }

        [Test, AutoData]
        public void Add_ignores_duplicate_items(ConcurrentList<string> sut, string item)
        {
            sut.Add(item);

            Assume.That(sut.Contains(item), Is.True);

            sut.Add(item);

            Assume.That(sut.Contains(item), Is.True);

            var result = sut.TryRemoveItem(item);

            Assert.That(result, Is.True);
            Assert.That(sut.Contains(item), Is.False);
        }

        [Test, AutoData]
        public void TryRemoveItem_removes_items_if_exists(ConcurrentList<string> sut, string item)
        {
            sut.Add(item);

            Assume.That(sut.Contains(item), Is.True);

            var result = sut.TryRemoveItem(item);

            Assert.That(result, Is.True);
            Assert.That(sut.Contains(item), Is.False);
        }
    }
}