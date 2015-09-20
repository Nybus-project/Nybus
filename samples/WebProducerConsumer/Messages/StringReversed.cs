using Nybus;

namespace Messages
{
    public class StringReversed : IEvent
    {
        public string Result { get; set; }

        public double TimeSlept { get; set; }
    }
}