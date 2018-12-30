using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Nybus.Utils;
using Types;

namespace PureSender
{
    class Program
    {
        static async Task Main(string[] args) => await CommandLineApplication.ExecuteAsync<Program>(args);

        [Option(CommandOptionType.SingleValue, LongName = "events", ShortName = "e")]
        public int Events { get; }

        [Option(CommandOptionType.SingleValue, LongName = "commands", ShortName = "c")]
        public int Commands { get; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task OnExecuteAsync()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

            services.AddNybus(cfg => cfg.UseRabbitMqBusEngine());

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var host = serviceProvider.GetRequiredService<NybusHost>();

            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("Starting host");
                await host.StartAsync();

                for (var i = 0; i < Commands; i++)
                {
                    await host.InvokeCommandAsync(new TestCommand
                    {
                        Message = $"Command {i} {Clock.Default.Now:yyyyMMdd-hhmmss}"
                    });

                    logger.LogInformation($"Command {i} sent");
                }

                for (var i = 0; i < Events; i++)
                {
                    await host.RaiseEventAsync(new TestEvent
                    {
                        Message = $"Event {i} {Clock.Default.Now:yyyyMMdd-hhmmss}"
                    });

                    logger.LogInformation($"Event {i} sent");
                }

                await host.StopAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
            }

            Console.ResetColor();
        }
    }
}
