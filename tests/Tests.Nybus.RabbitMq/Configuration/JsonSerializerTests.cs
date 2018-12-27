using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = Nybus.Configuration.JsonSerializer;

namespace Tests.Configuration
{
    [TestFixture]
    public class JsonSerializerTests
    {
        [Test, AutoMoqData]
        public void SerializeObject_returns_byte_representation_of_object(JsonSerializer sut, FirstTestCommand testObject)
        {
            var json = JsonConvert.SerializeObject(testObject);
            var bytes = Encoding.UTF8.GetBytes(json);

            var result = sut.SerializeObject(testObject, Encoding.UTF8);

            CollectionAssert.AreEqual(bytes, result);
        }

        [Test, AutoMoqData]
        public void DeserializeObject_returns_object_from_byte_representation(JsonSerializer sut, FirstTestCommand testObject)
        {
            var json = JsonConvert.SerializeObject(testObject);
            var bytes = Encoding.UTF8.GetBytes(json);

            var result = sut.DeserializeObject(bytes, typeof(FirstTestCommand), Encoding.UTF8) as FirstTestCommand;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Message, Is.EqualTo(testObject.Message));
        }
    }
}
