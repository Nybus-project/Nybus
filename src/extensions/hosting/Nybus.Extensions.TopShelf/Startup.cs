using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Topshelf;
using Topshelf.Runtime;

namespace Nybus
{
    public abstract class Startup
    {
        public virtual void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder) { }

        public virtual void ConfigureServices(StartupContext context, IServiceCollection services) { }

        public virtual void ConfigureLogging(StartupContext context, ILoggingBuilder logging) { }

        public abstract IBusHost ConstructService(StartupContext context, IServiceProvider serviceProvider);

        public virtual bool OnStart(IBusHost host, HostControl control)
        {
            host.StartAsync().GetAwaiter().GetResult();

            return true;
        }

        public virtual bool OnStop(IBusHost host, HostControl control)
        {
            host.StopAsync().GetAwaiter().GetResult();

            return true;
        }
    }

    public class StartupContext
    {
        public StartupContext(HostSettings settings, IConfigurationRoot configuration)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public HostSettings Settings { get; }

        public IConfigurationRoot Configuration { get; }
    }
}
