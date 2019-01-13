using System;

namespace Nybus
{
    public interface IContext
    {
        DateTimeOffset ReceivedOn { get; }

        DateTimeOffset SentOn { get; }

        Guid CorrelationId { get; }

        Message Message { get; }
    }
}
