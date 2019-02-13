using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Topshelf;

namespace NetFxWindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(configure =>
            {
                configure.UseStartup<NybusStartup>();

                configure.SetDisplayName("Nybus Windows Service");

                configure.SetServiceName("NybusWindowsService");

                configure.SetDescription("A Windows service running Nybus");

                configure.EnableServiceRecovery(rc => rc.RestartService(TimeSpan.FromMinutes(1))
                                                        .RestartService(TimeSpan.FromMinutes(5))
                                                        .RestartService(TimeSpan.FromMinutes(10))
                                                        .SetResetPeriod(1));

                configure.RunAsLocalService();

                configure.StartAutomaticallyDelayed();

                configure.SetStopTimeout(TimeSpan.FromMinutes(5));
            });

            host.Run();
        }
    }

    public class NybusStartup : Startup
    {
        public override IBusHost ConstructService(StartupContext context, IServiceProvider serviceProvider)
        {
            var host = serviceProvider.GetRequiredService<IBusHost>();

            return host;
        }

        public override void ConfigureServices(StartupContext context, IServiceCollection services)
        {
            services.AddNybus(nybus =>
            {
                nybus.UseConfiguration(context.Configuration);

                nybus.UseInMemoryBusEngine();
            });
        }

        public override void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddEnvironmentVariables();
        }

        public override void ConfigureLogging(StartupContext context, ILoggingBuilder logging)
        {
            logging.AddConfiguration(context.Configuration.GetSection("Logging"));

            logging.AddConsole();
        }
    }

}
