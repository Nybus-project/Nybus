using System;
using System.Collections.Generic;
using System.Text;
using Nybus.Utils;

namespace Tests
{
    public class TestClock : IClock
    {
        public TestClock(DateTimeOffset initialTime)
        {
            Now = initialTime;
        }

        public TestClock() : this(DateTimeOffset.UtcNow)
        {

        }

        public DateTimeOffset Now { get; private set; }

        public void AdvanceBy(TimeSpan interval)
        {
            Now += interval;
        }

        public void SetTo(DateTimeOffset newValue)
        {
            Now = newValue;
        }
    }
}
