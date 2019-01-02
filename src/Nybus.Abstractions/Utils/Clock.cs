using System;

namespace Nybus.Utils
{
    public static class Clock
    {
        public static IClock Default { get; private set; } = SystemClock.Instance;

        public static void Reset()
        {
            Default = SystemClock.Instance;
        }

        public static void SetTo(IClock clock)
        {
            Default = clock ?? throw new NotImplementedException(nameof(clock));
        }
    }

    public interface IClock
    {
        DateTimeOffset Now { get; }
    }

    public class SystemClock : IClock
    {
        private SystemClock() { }

        public DateTimeOffset Now => DateTimeOffset.UtcNow;

        public static readonly IClock Instance = new SystemClock(); 
    }
}
