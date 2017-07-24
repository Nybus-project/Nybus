using System;

namespace Nybus
{
    public static class Helpers
    {
        public static string StringfyGuid(Guid guid) => guid.ToString("N");

        public static string StringfyDateTimeOffset(DateTimeOffset dto) => dto.ToString("O");

    }
}
