using System;
using System.Collections.Generic;
using System.Text;
using Nybus;

namespace Tests
{
    public class FirstTestCommand : ICommand
    {
        public string Message { get; set; }
    }

    public class SecondTestCommand : ICommand { }

    public class FirstTestEvent : IEvent { }

    public class SecondTestEvent : IEvent { }
}
