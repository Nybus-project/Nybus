using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nybus.Utils
{
    public interface IMessageDescriptorStore
    {
        bool RegisterCommandType<TCommand>()
            where TCommand : class, ICommand;

        bool RegisterEventType<TEvent>()
            where TEvent : class, IEvent;

        bool FindCommandTypeForDescriptor(MessageDescriptor descriptor, out Type type);

        bool FindEventTypeForDescriptor(MessageDescriptor descriptor, out Type type);

        bool HasCommands();

        bool HasEvents();

        IEnumerable<Type> Commands { get; }

        IEnumerable<Type> Events { get; }
    }

    public class MessageDescriptor
    {
        public static MessageDescriptor CreateFromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var attribute = type.GetCustomAttribute<MessageAttribute>();

            if (attribute == null)
            {
                return new MessageDescriptor(type);
            }
            
            return new MessageDescriptor(attribute);
        }

        private static readonly char[] Separators = new []{':'};

        public static bool TryParse(string descriptorName, out MessageDescriptor descriptor)
        {
            if (descriptorName == null)
            {
                descriptor = null;
                return false;
            }

            var strings = descriptorName.Split(':');

            if (strings.Length != 2)
            {
                descriptor = null;
                return false;
            }

            descriptor = new MessageDescriptor(strings[1], strings[0]);
            return true;
        }

        public MessageDescriptor(MessageAttribute attribute) : this (attribute.Name, attribute.Namespace) { }

        public MessageDescriptor(Type type) : this (type.Name, type.Namespace) { }

        public MessageDescriptor(string name, string @namespace)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }

        public string Name { get; }

        public string Namespace { get; }

        public override string ToString() => $"{Namespace}:{Name}";

        public static implicit operator string(MessageDescriptor descriptor) => descriptor.ToString();

        public static readonly IEqualityComparer<MessageDescriptor> EqualityComparer = new MessageDescriptorEqualityComparer();

        private class MessageDescriptorEqualityComparer : IEqualityComparer<MessageDescriptor>
        {
            public bool Equals(MessageDescriptor x, MessageDescriptor y)
            {
                if (x == null && y == null) return true;

                if ((x != null) != (y != null)) return false;

                return string.Equals(x.ToString(), y.ToString(), StringComparison.Ordinal);
            }

            public int GetHashCode(MessageDescriptor obj)
            {
                return obj.ToString().GetHashCode();
            }
        }
    }
}