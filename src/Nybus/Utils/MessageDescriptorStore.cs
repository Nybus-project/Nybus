using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nybus.Utils
{
    public class MessageDescriptorStore : IMessageDescriptorStore
    {
        private readonly IDictionary<Type, MessageDescriptor> _descriptorsByType = new Dictionary<Type, MessageDescriptor>();
        private readonly IDictionary<MessageDescriptor, Type> _typesByDescriptor = new Dictionary<MessageDescriptor, Type>(MessageDescriptor.EqualityComparer);

        private readonly object _lock = new object();
        
        public bool RegisterType(Type type)
        {
            lock (_lock)
            {
                if (!_descriptorsByType.ContainsKey(type))
                {
                    var descriptor = GetDescriptorForType(type);

                    if (!_typesByDescriptor.ContainsKey(descriptor))
                    {
                        _descriptorsByType.Add(type, descriptor);
                        _typesByDescriptor.Add(descriptor, type);

                        return true;
                    }
                }

                return false;
            }
        }

        private MessageDescriptor GetDescriptorForType(Type type)
        {
            var attribute = type.GetCustomAttribute<MessageAttribute>();

            if (attribute == null)
            {
                return type;
            }

            return MessageDescriptor.CreateFromAttribute(attribute);
        }

        public bool TryGetDescriptorForType(Type type, out MessageDescriptor descriptor) => _descriptorsByType.TryGetValue(type, out descriptor);

        public bool TryGetTypeForDescriptor(MessageDescriptor descriptor, out Type type) => _typesByDescriptor.TryGetValue(descriptor, out type);
    }
}