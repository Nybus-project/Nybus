using System;
using System.Collections.Generic;

namespace Nybus.Utils
{
    public interface IMessageDescriptorStore
    {
        bool RegisterType(Type type);

        bool TryGetDescriptorForType(Type type, out MessageDescriptor descriptor);

        bool TryGetTypeForDescriptor(MessageDescriptor descriptor, out Type type);
    }

    public class MessageDescriptor
    {
        public static MessageDescriptor CreateFromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return new MessageDescriptor(type.Name, type.Namespace);
        }

        public static MessageDescriptor CreateFromAttribute (MessageAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            return new MessageDescriptor(attribute.Name, attribute.Namespace);
        }


        private static readonly char[] Separators = new []{':'};

        public static bool TryParse(string descriptorName, out MessageDescriptor descriptor)
        {
            var strings = descriptorName.Split(':');

            if (strings.Length != 2)
            {
                descriptor = null;
                return false;
            }

            descriptor = new MessageDescriptor(strings[1], strings[0]);
            return true;
        }

        public MessageDescriptor(string name, string @namespace)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }

        public string Name { get; }

        public string Namespace { get; }

        public override string ToString() => $"{Namespace}:{Name}";

        public static implicit operator MessageDescriptor(Type type) => CreateFromType(type);

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