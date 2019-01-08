using System;
using Newtonsoft.Json;

namespace Nybus.Configuration
{
    public interface ISerializer
    {
        string SerializeObject(object item);

        object DeserializeObject(string item, Type type);
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

        public string SerializeObject(object item)
        {
            return JsonConvert.SerializeObject(item, _settings);
        }

        public object DeserializeObject(string item, Type type)
        {
            return JsonConvert.DeserializeObject(item, type, _settings);
        }
    }
}