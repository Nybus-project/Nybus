using System;
using Nybus;

namespace Tests
{
    public class TestCommand : ICommand
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }
}