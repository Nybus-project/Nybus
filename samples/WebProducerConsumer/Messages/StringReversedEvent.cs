using Nybus;

namespace Messages
{
    public class StringReversedEvent : IEvent
    {
        public string Result { get; set; }

        public double TimeSlept { get; set; }
    }
}