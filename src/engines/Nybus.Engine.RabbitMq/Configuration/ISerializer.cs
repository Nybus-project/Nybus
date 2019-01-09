using System;
using System.Text;
using Newtonsoft.Json;

namespace Nybus.Configuration
{
    public interface ISerializer
    {
        byte[] SerializeObject(object item, Encoding encoding);

        object DeserializeObject(byte[] bytes, Type type, Encoding encoding);
    }

    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer() : this(new JsonSerializerSettings())
        {

        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public byte[] SerializeObject(object item, Encoding encoding)
        {
            var json = JsonConvert.SerializeObject(item, _settings);
            return encoding.GetBytes(json);
        }

        public object DeserializeObject(byte[] bytes, Type type, Encoding encoding)
        {
            var json = encoding.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type, _settings);
        }
    }
}