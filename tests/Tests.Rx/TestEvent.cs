using System;
using Nybus;

namespace Tests
{
    public class TestEvent : IEvent
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }
}