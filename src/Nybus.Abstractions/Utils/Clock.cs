using System;
using System.Collections.Generic;
using System.Text;

namespace Nybus.Utils
{
    public static class Clock
    {
        static Clock()
        {
            Default = new SystemClock();
        }
        public static IClock Default { get; set; }
    }

    public interface IClock
    {
        DateTimeOffset Now { get; }
    }

    public class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.UtcNow;
    }
}
