using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nybus.Utils
{
    public class MessageDescriptorStore : IMessageDescriptorStore
    {
        private readonly TwoWayDictionary<Type, MessageDescriptor> _commandTypes = new TwoWayDictionary<Type, MessageDescriptor>(MessageDescriptor.EqualityComparer);
        private readonly TwoWayDictionary<Type, MessageDescriptor> _eventTypes = new TwoWayDictionary<Type, MessageDescriptor>(MessageDescriptor.EqualityComparer);

        private readonly object _lock = new object();
        
        public bool RegisterCommandType<TCommand>() where TCommand : class, ICommand
        {
            return RegisterType(_commandTypes, typeof(TCommand));
        }

        public bool RegisterEventType<TEvent>() where TEvent : class, IEvent
        {
            return RegisterType(_eventTypes, typeof(TEvent));
        }

        private bool RegisterType(TwoWayDictionary<Type, MessageDescriptor> registeredTypes, Type type)
        {
            lock (_lock)
            {
                if (!registeredTypes.ContainsKey(type) && TryGetDescriptorFromAttribute(type, out var descriptor))
                {
                    registeredTypes.Add(type, descriptor);

                    return true;
                }

                return false;
            }
        }

        private bool TryGetDescriptorFromAttribute(Type type, out MessageDescriptor descriptor)
        {
            var attribute = type.GetCustomAttribute<MessageAttribute>();

            if (attribute == null)
            {
                descriptor = MessageDescriptor.CreateFromType(type);
                return true;
            }

            descriptor = MessageDescriptor.CreateFromAttribute(attribute);
            return true;
        }

        public bool FindCommandTypeForDescriptor(MessageDescriptor descriptor, out Type type) => FindTypeForDescriptor(_commandTypes, descriptor, out type);

        public bool FindEventTypeForDescriptor(MessageDescriptor descriptor, out Type type) => FindTypeForDescriptor(_eventTypes, descriptor, out type);

        private static bool FindTypeForDescriptor(TwoWayDictionary<Type, MessageDescriptor> registeredTypes, MessageDescriptor descriptor, out Type type) => registeredTypes.TryGetValue(descriptor, out type);

        public bool HasCommands()
        {
            return _commandTypes.FirstItems.Count > 0;
        }

        public bool HasEvents()
        {
            return _eventTypes.FirstItems.Count > 0;
        }

        public IEnumerable<Type> Commands => _commandTypes.FirstItems;

        public IEnumerable<Type> Events => _eventTypes.FirstItems;
    }
}