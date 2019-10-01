using System;
using Nybus;

namespace Types
{
    public class DoSomethingCommand : ICommand
    {
        public string WhatToDo { get; set; }
    }

    public class SomethingDoneEvent : IEvent
    {
        public string WhatWasDone { get; set; }
    }
}
