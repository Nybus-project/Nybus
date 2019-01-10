using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture.Idioms;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class TwoWayDictionaryTests
    {
        [Test, AutoMoqData]
        public void Constructors_are_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(TwoWayDictionary<int, string>).GetConstructors());
        }

        [Test, AutoMoqData]
        public void Constructor_can_accept_equality_comparer_T1(IEqualityComparer<int> equalityComparer)
        {
            var sut = new TwoWayDictionary<int, string>(equalityComparer);
        }

        [Test, AutoMoqData]
        public void Constructor_can_accept_equality_comparer_T2(IEqualityComparer<string> equalityComparer)
        {
            var sut = new TwoWayDictionary<int, string>(equalityComparer);
        }

        [Test, AutoMoqData]
        public void Clear_removes_all_items(TwoWayDictionary<int, string> sut, (int, string)[] items)
        {
            foreach (var (item1, item2) in items)
            {
                sut.Add(item1, item2);
            }

            Assume.That(sut.Count(), Is.GreaterThan(0));

            sut.Clear();

            Assert.That(sut.Count(), Is.EqualTo(0));
        }

        [Test, AutoMoqData]
        public void ContainsKey_returns_true_if_item_is_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            var (item1, item2) = item;
            sut.Add(item1, item2);

            Assert.That(sut.ContainsKey(item1), Is.True);

            Assert.That(sut.ContainsKey(item2), Is.True);
        }

        [Test, AutoMoqData]
        public void ContainsKey_returns_false_if_item_is_not_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            var (item1, item2) = item;

            Assert.That(sut.ContainsKey(item1), Is.False);

            Assert.That(sut.ContainsKey(item2), Is.False);
        }

        [Test, AutoMoqData]
        public void RemoveItem_can_remove_item_by_its_first_key(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assume.That(sut.ContainsKey(item.Item1));

            Assert.That(sut.Remove(item.Item1));
            Assert.That(sut.ContainsKey(item.Item1), Is.False);
        }

        [Test, AutoMoqData]
        public void RemoveItem_can_remove_item_by_its_second_key(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assume.That(sut.ContainsKey(item.Item2));

            Assert.That(sut.Remove(item.Item2));
            Assert.That(sut.ContainsKey(item.Item2), Is.False);
        }

        [Test, AutoMoqData]
        public void Remove_T1_return_false_if_item_is_not_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            Assert.That(sut.Remove(item.Item1), Is.False);
        }

        [Test, AutoMoqData]
        public void Remove_T2_return_false_if_item_is_not_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            Assert.That(sut.Remove(item.Item2), Is.False);
        }

        [Test, AutoMoqData]
        public void TryGetValue_by_T1_returns_other_value_if_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assert.That(sut.TryGetValue(item.Item1, out var value), Is.True);
            Assert.That(value, Is.EqualTo(item.Item2));
        }

        [Test, AutoMoqData]
        public void TryGetValue_by_T2_returns_other_value_if_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assert.That(sut.TryGetValue(item.Item2, out var value), Is.True);
            Assert.That(value, Is.EqualTo(item.Item1));
        }

        [Test, AutoMoqData]
        public void TryGetValue_by_T1_returns_false_if_not_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            Assert.That(sut.TryGetValue(item.Item1, out var value), Is.False);
            Assert.That(value, Is.EqualTo(default(string)));
        }

        [Test, AutoMoqData]
        public void TryGetValue_by_T2_returns_false_if_not_added(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            Assert.That(sut.TryGetValue(item.Item2, out var value), Is.False);
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test, AutoMoqData]
        public void Indexer_T1_can_access_existing_items(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assert.That(sut[item.Item1], Is.EqualTo(item.Item2));
        }

        [Test, AutoMoqData]
        public void Indexer_T2_can_access_existing_items(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assert.That(sut[item.Item2], Is.EqualTo(item.Item1));
        }

        [Test, AutoMoqData]
        public void Indexer_T1_can_persist_item(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut[item.Item1] = item.Item2;

            Assert.That(sut.FirstItems, Contains.Item(item.Item1));
            Assert.That(sut.SecondItems, Contains.Item(item.Item2));
        }

        [Test, AutoMoqData]
        public void Indexer_T2_can_persist_item(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut[item.Item2] = item.Item1;

            Assert.That(sut.FirstItems, Contains.Item(item.Item1));
            Assert.That(sut.SecondItems, Contains.Item(item.Item2));
        }

        [Test, AutoMoqData]
        public void GetEnumerator_returns_added_items(TwoWayDictionary<int, string> sut, (int, string) item)
        {
            sut.Add(item.Item1, item.Item2);

            Assert.That(((IEnumerable)sut).GetEnumerator(), Is.InstanceOf<IEnumerable<(int, string)>>());
        }
    }
}
