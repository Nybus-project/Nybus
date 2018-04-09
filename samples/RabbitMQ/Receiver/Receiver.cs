using System;
using System.Threading;
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
                cfg.UseRabbitMqBusEngine(configure =>
                {
                    configure.Connection(connection =>
                    {
                        connection.HostName = "localhost";
                        connection.UserName = "guest";
                        connection.Password = "guest";
                    });

                    configure.Connection("RabbitMq");
                });

                cfg.SubscribeToCommand<TestCommand>(async (d, msg) =>
                {
                    Console.WriteLine($"Processed {msg.Command.Message}");
                    await d.RaiseEventAsync(new TestEvent());

                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                });

                cfg.SubscribeToEvent<TestEvent>(async (d, msg) =>
                {
                    Console.WriteLine($"Processed {msg.Event.Message}");
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

    public class TestEvent : IEvent
    {
        public string Message { get; set; }
    }
}
