using Nybus;

namespace NLogSampleApp
{
    public class MessageReceived : IEvent
    {
        public string Message { get; set; }
    }
}