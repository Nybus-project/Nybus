using Nybus;

namespace NLogSampleApp
{
    public class SendMessage : ICommand
    {
        public string Message { get; set; }
    }
}