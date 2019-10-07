using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Nybus.Configuration;
using Nybus.MassTransit;
using Types;

namespace PureSender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Trace));

            services.AddNybus(nybus =>
            {
                nybus.UseMassTransitWithRabbitMq(c =>
                {
                    c.ConfigureMassTransit(mt =>
                    {
                        mt.Host(new Uri("rabbitmq://localhost"), h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });
                    });
                });

                nybus.SubscribeToEvent<SomethingDoneEvent>(async (dispatcher, context) =>
                {
                    await Console.Out.WriteLineAsync($"Something was done: {context.Event.WhatWasDone}");
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(new DoSomethingCommand
            {
                WhatToDo = "Whatever you want"
            });

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
