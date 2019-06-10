using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Types;

namespace ErrorFilters
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

        [Option(CommandOptionType.SingleValue, LongName = "retries", ShortName = "r")]
        public int Retries { get; set; } = 5;

        public async Task OnExecuteAsync()
        {
            var settings = new Dictionary<string, string>
            {
                ["Nybus:RabbitMq:Connection:HostName"] = HostName,
                ["Nybus:RabbitMq:Connection:Username"] = Username,
                ["Nybus:RabbitMq:Connection:Password"] = Password,
                ["Nybus:RabbitMq:Connection:VirtualHost"] = VirtualHost,
                ["Nybus:CommandErrorFilters:0:type"] = "retry",
                ["Nybus:CommandErrorFilters:0:maxRetries"] = Retries.ToString(),
                ["Nybus:EventErrorFilters:0:type"] = "retry",
                ["Nybus:EventErrorFilters:0:maxRetries"] = Retries.ToString(),
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(settings);

            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            services.AddNybus(nybus =>
            {
                nybus.UseConfiguration(configuration, "Nybus");

                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.UseConfiguration("RabbitMq");
                });

                nybus.SubscribeToCommand<TestCommand>(async (dispatcher, context) =>
                {
                    Console.WriteLine($"Processed command {context.Command.Message}");
                    await dispatcher.RaiseEventAsync(new TestCommandReceived { Message = $@"Received message ""{context.Command.Message}""" });

                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                    throw new Exception("Hello world");
                });

                nybus.SubscribeToEvent<TestEvent>((d, msg) =>
                {
                    Console.WriteLine($"Processed event {msg.Event.Message}");

                    throw new Exception("Hello world");
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
