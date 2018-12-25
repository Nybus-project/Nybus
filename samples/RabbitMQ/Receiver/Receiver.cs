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
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

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
                    Console.WriteLine($"Processed command {msg.Command.Message}");
                    await d.RaiseEventAsync(new TestEvent { Message = msg.Command.Message });

                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                });

                cfg.SubscribeToEvent<TestEvent>((d, msg) =>
                {
                    Console.WriteLine($"Processed event {msg.Event.Message}");

                    return Task.CompletedTask;
                });
            });

            var serviceProvider = services.BuildServiceProvider();

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
