using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreRabbitMq.Handlers;
using Nybus;
using Nybus.Configuration;

namespace NetCoreRabbitMq
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();

            builder.ConfigureHostConfiguration(configuration =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());

                configuration.AddJsonFile("hostsettings.json", true);
                configuration.AddEnvironmentVariables(prefix: "NYBUS_");
                configuration.AddCommandLine(args);
            });

            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json", true);
                configuration.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                configuration.AddEnvironmentVariables(prefix: "NYBUS_");
                configuration.AddCommandLine(args);
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<NybusHostedService>();

                services.AddNybus(nybus =>
                {
                    nybus.UseConfiguration(context.Configuration);

                    nybus.UseRabbitMqBusEngine(rabbitMq =>
                    {
                        rabbitMq.UseConfiguration();

                        rabbitMq.Configure(configuration => configuration.CommandQueueFactory = new StaticQueueFactory("NetCoreRabbitMq"));
                    });

                    /* EVENTS */

                    /* This will subscribe the event TestEvent to any IEventHandler<TestEvent> available */
                    nybus.SubscribeToEvent<TestEvent>();

                    /* This will subscribe the event TestEvent to an instance of TestEventHandler */
                    // nybus.SubscribeToEvent<TestEvent, TestEventHandler>();

                    /* This will subscribe the event TestEvent to the asynchronous delegate */
                    // nybus.SubscribeToEvent<TestEvent>(async (dispatcher, eventContext) => { await DoSomethingAsync(); });

                    /* This will subscribe the event TestEvent to the synchronous delegate */
                    // nybus.SubscribeToEvent<TestEvent>((dispatcher, eventContext) => { DoSomething(); });


                    /* COMMANDS */

                    /* This will subscribe the command TestCommand to any ICommandHandler<TestCommand> available */
                    nybus.SubscribeToCommand<TestCommand>();

                    /* This will subscribe the command TestCommand to an instance of TestCommandHandler */
                    // nybus.SubscribeToCommand<TestCommand, TestCommandHandler>();

                    /* This will subscribe the command TestCommand to the asynchronous delegate */
                    // nybus.SubscribeToCommand<TestCommand>(async (dispatcher, commandContext) => { await DoSomethingAsync(); });

                    /* This will subscribe the command TestCommand to the synchronous delegate */
                    // nybus.SubscribeToCommand<TestCommand>((dispatcher, commandContext) => { DoSomething(); });
                });

                /* EVENTS */

                /* This will register the event handler TestEventHandler as a handler for TestEvent */
                // services.AddEventHandler<TestEvent, TestEventHandler>();

                /* This will register the event handler TestEventHandler as a handler for all supported events */
                services.AddEventHandler<TestEventHandler>();


                /* COMMANDS */

                /* This will register the command handler TestCommandHandler as a handler for TestCommand */
                // services.AddCommandHandler<TestCommand, TestCommandHandler>();

                /* This will register the command handler TestCommandHandler as a handler for all supported Commands */
                services.AddCommandHandler<TestCommandHandler>();
            });

            builder.ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
            });

            var host = builder.Build();

            await host.RunAsync();
        }
    }

    public class TestEvent : IEvent { }

    public class TestCommand : ICommand { }
}
