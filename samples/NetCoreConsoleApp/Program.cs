using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nybus;
using System;
using System.Threading.Tasks;

namespace NetCoreConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();

            //services.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>();


            services.AddNybus(cfg =>
            {
                cfg.UseInMemoryBusEngine();

                cfg.SubscribeToEvent<TestEvent, TestEventHandler>();

                cfg.SubscribeToCommand<TestCommand>(async (b, ctx) => {

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