using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nybus;
using System;
using System.Threading.Tasks;
using Nybus.Policies;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace NetCoreConsoleApp
{
    class Program
    {
        static void Main(string[] args)
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

            services.AddTransient<RetryCommandErrorPolicy>();
            services.AddTransient<RetryEventErrorPolicy>();
            services.Configure<RetryCommandErrorPolicyOptions>(configuration.GetSection("RetryCommandError"));
            services.Configure<RetryEventErrorPolicyOptions>(configuration.GetSection("RetryEventError"));

            //services.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>();

            services.AddNybus(cfg =>
            {
                cfg.CustomizeOptions(options => 
                {
                    options.SetCommandErrorPolicy<RetryCommandErrorPolicy>();
                    options.SetEventErrorPolicy<RetryEventErrorPolicy>();
                });

                cfg.UseInMemoryBusEngine();

                cfg.SubscribeToEvent<TestEvent, TestEventHandler>();

                cfg.SubscribeToCommand<TestCommand>(async (b, ctx) => 
                {
                    throw new Exception("Error");

                    await b.RaiseEventAsync(new TestEvent
                    {
                        Message = $@"Received ""{ctx.Command.Message}"""
                    }, ctx.CorrelationId);
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(LogLevel.Trace);

            IBus bus = serviceProvider.GetRequiredService<IBus>();

            bus.StartAsync().GetAwaiter().GetResult();

            bus.InvokeCommandAsync(new TestCommand { Message = "Hello World" });

            bus.InvokeCommandAsync(new TestCommand { Message = "Foo bar" });

            bus.StopAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }
    }
}