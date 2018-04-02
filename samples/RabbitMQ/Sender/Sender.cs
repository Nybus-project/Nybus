﻿using System;
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
                cfg.UseBusEngine<RabbitMQBusEngine>(svc =>
                {
                    svc.AddSingleton(new RabbitMqBusEngineOptions
                    {
                        CommandQueueName = "test-queue"
                    });
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(LogLevel.Trace);

            var host = serviceProvider.GetRequiredService<NybusHost>();

            try
            {
                await host.StartAsync();

                await host.InvokeCommandAsync(new TestCommand { Message = "Hello world" });

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

}