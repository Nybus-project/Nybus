using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nybus;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using Nybus.Utils;

namespace NetCoreConsoleApp
{
    class Program
    {
        private static int _counter;

        static async Task Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Nybus:RetryPolicy:MaxRetries"] = "5",
            });

            IConfiguration configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();

            services.AddNybus(cfg =>
            {
                cfg.UseConfiguration(configuration);

                cfg.UseInMemoryBusEngine();

                cfg.SubscribeToEvent<TestEvent, TestEventHandler>();

                cfg.SubscribeToCommand<TestCommand>(async (b, ctx) =>
                {
                    Interlocked.Increment(ref _counter);

                    if (_counter > 2 && _counter < 6)
                    {
                        throw new Exception("Error");
                    }

                    await b.RaiseEventAsync(new TestEvent
                    {
                        Message = $@"Received ""{ctx.Command.Message}"""
                    });
                });

                cfg.UseRetryErrorPolicy();
            });

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(LogLevel.Trace);

            try
            {
                var host = serviceProvider.GetRequiredService<IBusHost>();

                var bus = serviceProvider.GetRequiredService<IBus>();

                await host.StartAsync();

                if (args.Length == 1 && int.TryParse(args[0], out var messages))
                {
                    for (var i = 0; i < messages; i++)
                    {
                        await bus.InvokeCommandAsync(new TestCommand { Message = $"Message {i} {Clock.Default.Now:yyyyMMdd-hhmmss}" });
                    }
                }
                else
                {
                    await bus.InvokeCommandAsync(new TestCommand { Message = "Hello World" });

                    await bus.InvokeCommandAsync(new TestCommand { Message = "Foo bar" });

                    await bus.InvokeCommandAsync(new TestCommand { Message = "Another message" });
                }

                await host.StopAsync();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}