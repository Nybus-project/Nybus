using System;

namespace Nybus.Configuration
{
    public interface ICorrelationIdGenerator
    {
        Guid Generate();
    }

    public class NewGuidCorrelationIdGenerator : ICorrelationIdGenerator
    {
        public Guid Generate() => Guid.NewGuid();
    }
}