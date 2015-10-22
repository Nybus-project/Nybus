using System;

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
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
