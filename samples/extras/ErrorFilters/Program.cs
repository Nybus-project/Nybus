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
                ["Nybus:CommandErrorFilters:Type"] = "retry",
                ["Nybus:CommandErrorFilters:MaxRetries"] = Retries.ToString(),
                ["Nybus:EventErrorFilters:Type"] = "retry",
                ["Nybus:EventErrorFilters:MaxRetries"] = Retries.ToString()
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(settings);

            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

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
                });

                nybus.SubscribeToEvent<TestEvent>((d, msg) =>
                {
                    Console.WriteLine($"Processed event {msg.Event.Message}");
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
