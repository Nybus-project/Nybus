using Castle.MicroKernel;
using Nybus.Configuration;
using Nybus.Container;

namespace Nybus
{
    public static class WindsorExtensions
    {
        public static void UseCastleWindsor(this NybusOptions options, IKernel kernel)
        {
            options.Container = new WindsorBusContainer(kernel);
        }
         
    }
}