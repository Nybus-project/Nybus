using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Nybus.Configuration;
using Nybus.Utils;

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
                cfg.UseRabbitMqBusEngine();
            });

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var host = serviceProvider.GetRequiredService<NybusHost>();

            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                await host.StartAsync();

                if (args.Length == 1 && int.TryParse(args[0], out var messages))
                {
                    for (var i = 1; i <= messages; i++)
                    {
                        //await host.InvokeCommandAsync(new TestCommand { Message = $"Message {i} {Clock.Default.Now:yyyyMMdd-hhmmss}" });

                        await host.RaiseEventAsync(new TestEvent { Message = $"Message {i} {Clock.Default.Now:yyyyMMdd-hhmmss}" });
                        logger.LogInformation($"Message {i} sent");
                    }
                }
                else
                {
                    await host.InvokeCommandAsync(new TestCommand { Message = "Hello World" });

                    await host.InvokeCommandAsync(new TestCommand { Message = "Foo bar" });

                    await host.InvokeCommandAsync(new TestCommand { Message = "Another message" });
                }

                await host.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Bye!");
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

        public override string ToString() => Message;
    }

}
