using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nybus;
using System;
using System.Threading.Tasks;
using Nybus.Policies;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Nybus.Configuration;

namespace NetCoreConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                //["Nybus:Policies:CommandError:Retry:MaxRetries"] = "5",
                //["Nybus:Policies:EventError:Retry:MaxRetries"] = "5",
                ["RetryCommandError:MaxRetries"] = "5",
                ["RetryEventError:MaxRetries"] = "5"
            });

            IConfiguration configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();

            services.AddTransient<RetryErrorPolicy>();
            services.Configure<RetryErrorPolicyOptions>(configuration.GetSection("RetryCommandError"));

            services.AddNybus(cfg =>
            {
                //cfg.CustomizeOptions(options => 
                //{
                //    //options.SetErrorPolicy<RetryErrorPolicy>();
                //    //options.SetErrorPolicy<NoopErrorPolicy>();
                //});

                cfg.UseInMemoryBusEngine();

                cfg.SubscribeToEvent<TestEvent, TestEventHandler>();

                cfg.SubscribeToCommand<TestCommand>(async (b, ctx) =>
                {
                    if (ctx.ReceivedOn.Second % 2 == 0)
                    {
                        throw new Exception("Error");
                    }

                    await b.RaiseEventAsync(new TestEvent
                    {
                        Message = $@"Received ""{ctx.Command.Message}"""
                    });
                });

                //cfg.RegisterPolicy<RetryErrorPolicy>();
            });

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(LogLevel.Trace);

            try
            {
                var host = serviceProvider.GetRequiredService<IBusHost>();

                var bus = serviceProvider.GetRequiredService<IBus>();

                await host.StartAsync();

                await bus.InvokeCommandAsync(new TestCommand { Message = "Hello World" });

                await bus.InvokeCommandAsync(new TestCommand { Message = "Foo bar" });

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