using System;
using Castle.MicroKernel;
using Nybus.Configuration;
using Nybus.Container;

namespace Nybus
{
    public static class WindsorExtensions
    {
        public static void UseCastleWindsor(this NybusOptions options, IKernel kernel)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            options.Container = new WindsorBusContainer(kernel);
        }
         
    }
}