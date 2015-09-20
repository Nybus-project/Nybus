using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Nybus.Utils;
using Ploeh.AutoFixture;

namespace Tests.Utils
{
    [TestFixture]
    public class ObjectToDictionaryTests
    {
        private IFixture fixture = new Fixture();

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        [Test]
        public void Convert_returns_a_dictionary_with_items()
        {
            var data = new
            {
                text = fixture.Create<string>(),
                value = fixture.Create<int>(),
                time = fixture.Create<DateTimeOffset>(),
                flag = fixture.Create<bool>()
            };

            var dictionary = ObjectToDictionary.Convert(data);

            Assert.That(dictionary, Is.Not.Null);
            Assert.That(dictionary[nameof(data.text)], Is.EqualTo(data.text));
            Assert.That(dictionary[nameof(data.value)], Is.EqualTo(data.value));
            Assert.That(dictionary[nameof(data.time)], Is.EqualTo(data.time));
            Assert.That(dictionary[nameof(data.flag)], Is.EqualTo(data.flag));
        }

        [Test]
        public void Convert_uses_cache_to_spare_calls()
        {
            var data = new
            {
                text = fixture.Create<string>(),
                value = fixture.Create<int>(),
                time = fixture.Create<DateTimeOffset>(),
                flag = fixture.Create<bool>()
            };

            var dictionary = ObjectToDictionary.Convert(data);

            Assert.That(ObjectToDictionary.Cache, Is.Not.Empty);
            Assert.That(ObjectToDictionary.Cache[data.GetType()], Is.Not.Null);
        }

        [Test]
        public void Convert_returns_empty_dictionary_when_null()
        {
            var dictionary = ObjectToDictionary.Convert(null);

            Assert.That(dictionary, Is.Not.Null);
            Assert.That(dictionary, Is.Empty);
        }

        [Test]
        public void Convert_returns_original_object_if_dictionary()
        {
            var data = fixture.Create<Dictionary<string, object>>();

            var dictionary = ObjectToDictionary.Convert(data);

            Assert.That(dictionary, Is.SameAs(data));
        }
    }
}
