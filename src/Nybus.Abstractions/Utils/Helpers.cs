using System;

namespace Nybus.Utils
{
    public static class Helpers
    {
        public static string Stringfy(this int value) => value.ToString("D");

        public static string Stringfy(this Guid guid) => guid.ToString("N");

        public static string Stringfy(this DateTimeOffset dto) => dto.ToString("O");

    }
}
