using System;
using System.Threading.Tasks;
using Nybus;
using Nybus.Logging;

namespace NLogSampleApp
{
    public class MessageReceivedHandler : IEventHandler<MessageReceived>
    {
        private readonly ILogger _logger;

        public MessageReceivedHandler(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public async Task Handle(EventContext<MessageReceived> eventMessage)
        {
            await _logger.LogAsync(LogLevel.Info, $"Message received {eventMessage.Message.Message}");
        }
    }
}