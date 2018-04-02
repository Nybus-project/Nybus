using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Nybus.Configuration;

namespace RabbitMQ
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();

            services.AddNybus(cfg =>
            {
                cfg.UseBusEngine<RabbitMQBusEngine>(svc => svc.AddSingleton(new RabbitMqBusEngineOptions { CommandQueueName = "test-queue" }));

                cfg.SubscribeToCommand<TestCommand>(async (d, msg) =>
                {
                    Console.WriteLine($"Received {msg.Command.Message} at {msg.ReceivedOn:G}");
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(LogLevel.Trace);

            var host = serviceProvider.GetRequiredService<NybusHost>();

            try
            {
                await host.StartAsync();

                Console.WriteLine("Press <ENTER> to exit");
                Console.ReadLine();

                await host.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Hello World!");
        }
    }

    public class TestCommand : ICommand
    {
        public string Message { get; set; }

        public override string ToString() => Message;
    }
}
