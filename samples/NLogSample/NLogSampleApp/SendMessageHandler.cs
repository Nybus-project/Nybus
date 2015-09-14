using System;
using System.Threading.Tasks;
using Nybus;
using Nybus.Logging;

namespace NLogSampleApp
{
    public class SendMessageHandler : ICommandHandler<SendMessage>
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;

        public SendMessageHandler(IBus bus, ILogger logger)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _bus = bus;
            _logger = logger;
        }

        public async Task Handle(CommandContext<SendMessage> commandMessage)
        {
            await _logger.LogAsync(LogLevel.Info, $"Sending message {commandMessage.Message.Message}");

            await _bus.RaiseEvent(new MessageReceived { Message = commandMessage.Message.Message }, commandMessage.CorrelationId);
        }
    }
}