using Microsoft.Extensions.Logging;
using Nybus;
using System;
using System.Threading.Tasks;

namespace NetCoreConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Trace);

            var busEngine = new InMemoryBusEngine();

            IBus bus = new NybusBus(busEngine, loggerFactory.CreateLogger<NybusBus>());

            bus.SubscribeToCommand<TestCommand>(async ctx => {

                await bus.RaiseEventAsync(new TestEvent
                {
                    Message = $"Received: {ctx.Command.Message}"
                });
            });

            bus.SubscribeToEvent<TestEvent>(ctx => {

                Console.WriteLine(ctx.Event.Message);

                return Task.CompletedTask;
            });

            bus.StartAsync().GetAwaiter().GetResult();

            bus.InvokeCommandAsync(new TestCommand { Message = "Hello World" });

            Console.ReadLine();
        }
    }
}