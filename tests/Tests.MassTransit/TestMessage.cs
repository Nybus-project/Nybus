using System;
using Nybus;

namespace Tests
{
    public class TestMessage
    {
        public int Value { get; set; }

        public string Text { get; set; }

        public DateTimeOffset Time { get; set; }

        public bool Flag { get; set; }
    }

    public class TestEvent : IEvent
    {
        public int Value { get; set; }

        public string Text { get; set; }

        public DateTimeOffset Time { get; set; }

        public bool Flag { get; set; }

    }

    public class TestCommand : ICommand
    {
        public int Value { get; set; }

        public string Text { get; set; }

        public DateTimeOffset Time { get; set; }

        public bool Flag { get; set; }

    }
}