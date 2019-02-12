using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nybus;

namespace NetCoreApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();

            builder.ConfigureHostConfiguration(configuration =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());

                configuration.AddJsonFile("hostsettings.json", true);
                configuration.AddEnvironmentVariables(prefix: "NYBUS_");
                configuration.AddCommandLine(args);
            });

            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json", true);
                configuration.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                configuration.AddEnvironmentVariables(prefix: "NYBUS_");
                configuration.AddCommandLine(args);
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddNybus(nybus =>
                {
                    nybus.UseConfiguration(context.Configuration);

                    nybus.UseRabbitMqBusEngine(rabbitMq =>
                    {
                        rabbitMq.UseConfiguration();
                    });

                    nybus.SubscribeToEvent<TestEvent>();
                });

                services.AddEventHandler<TestEventHandler>();

                services.AddHostedService<NybusHostedService>();
            });

            builder.ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
            });

            var host = builder.Build();

            await host.RunAsync();
        }
    }

    public class TestEvent : IEvent { }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<TestEvent> incomingEvent)
        {
            return Task.CompletedTask;
        }
    }
}
