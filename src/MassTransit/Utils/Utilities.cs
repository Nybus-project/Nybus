using System;
using System.Reflection;

namespace Nybus.Utils
{
    internal static class Utilities
    {
        public static string GetUniqueNameForApplication()
        {
            if (Assembly.GetEntryAssembly() != null)
            {
                return Assembly.GetEntryAssembly().FullName.Hash();
            }

            return AppDomain.CurrentDomain.BaseDirectory.Hash();
        }
    }
}