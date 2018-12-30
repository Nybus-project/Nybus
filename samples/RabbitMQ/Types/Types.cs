using System;
using Nybus;

namespace Types
{
    public class TestCommand : ICommand
    {
        public string Message { get; set; }

        public override string ToString() => Message;
    }

    public class TestCommandReceived : IEvent
    {
        public string Message { get; set; }

        public override string ToString() => Message;

    }

    public class TestEvent : IEvent
    {
        public string Message { get; set; }

        public override string ToString() => Message;

    }
}
