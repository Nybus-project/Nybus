using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using RabbitMQ.Client;
using Types;

namespace ReceiverConfig
{
    class Program
    {
        static async Task Main(string[] args) => await CommandLineApplication.ExecuteAsync<Program>();

        [Option(CommandOptionType.SingleValue, LongName = "hostName")]
        public string HostName { get; set; } = "localhost";

        [Option(CommandOptionType.SingleValue, LongName = "username", ShortName = "u")]
        public string Username { get; set; } = "guest";

        [Option(CommandOptionType.SingleValue, LongName = "password", ShortName = "p")]
        public string Password { get; set; } = "guest";

        [Option(CommandOptionType.SingleValue, LongName = "virtualHost", ShortName = "v")]
        public string VirtualHost { get; set; } = "/";


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task OnExecuteAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

            services.AddNybus(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(c =>
                    {
                        c.ConnectionFactory = new ConnectionFactory
                        {
                            HostName = HostName,
                            UserName = Username,
                            Password = Password,
                            VirtualHost = VirtualHost
                        };
                    });
                });

                nybus.SubscribeToCommand<TestCommand>(async (dispatcher, context) =>
                {
                    Console.WriteLine($"Processed command {context.Command.Message}");
                    await dispatcher.RaiseEventAsync(new TestCommandReceived { Message = $@"Received message ""{context.Command.Message}""" });

                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                });

                nybus.SubscribeToEvent<TestEvent>((d, msg) =>
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
}
