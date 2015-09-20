using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Nybus;
using Nybus.Configuration;
using Nybus.Logging;
using Nybus.Utils;

namespace RxSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new NLogLogger(NLog.LogManager.GetLogger("Test"));

            var options = new NybusOptions
            {
                Logger = logger
            };

            var engine = new InMemoryBusEngine();

            var busBuilder = new NybusBusBuilder(engine, options);

            var consoleLocker = new object();

            IDisposable testEventHandle = busBuilder.ObserveEvent<TestEvent>().Buffer(TimeSpan.FromSeconds(1)).Subscribe(tc =>
            {
                lock (consoleLocker)
                {
                    Console.WriteLine($"Received {tc.Count} events during the last 1 seconds");
                    foreach (var i in tc)
                    {
                        Console.WriteLine($"\t{i.CorrelationId:D} - {i.Message.Id} - '{i.Message.Message}'");
                    }
                    Console.WriteLine();
                }
            });

            IDisposable testCommandHandle = busBuilder.ObserveCommand<TestCommand>().Buffer(10).Subscribe(tc =>
            {
                lock (consoleLocker)
                {
                    Console.WriteLine($"Received {tc.Count} commands");
                    foreach (var i in tc)
                    {
                        Console.WriteLine($"\t{i.CorrelationId:D} - {i.Message.Id} - '{i.Message.Message}'");
                    }
                    Console.WriteLine();
                }
            });


            IBus bus = busBuilder.Build();

            Task.WaitAll(bus.Start());

            Task.WhenAll(InvokeManyEvents(bus, 50), InvokeManyCommands(bus, 50)).WaitAndUnwrapException();

            Console.WriteLine("Press ENTER to exit.");

            Console.ReadLine();

            testEventHandle.Dispose();

            testCommandHandle.Dispose();

            Task.WaitAll(bus.Stop());

        }

        public static async Task InvokeManyCommands(IBus bus, int times)
        {
            for (int i = 0; i < times; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                await bus.InvokeCommand(new TestCommand
                {
                    Id = i,
                    Message = $"{Guid.NewGuid():D}"
                });
            }
        }

        public static async Task InvokeManyEvents(IBus bus, int times)
        {
            for (int i = 0; i < times; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                await bus.RaiseEvent(new TestEvent
                {
                    Id = i,
                    Message = $"{Guid.NewGuid():D}"
                });
            }
        }

    }

    public class TestCommand : ICommand
    {
        public string Message { get; set; }
        public int Id { get; set; }
    }

    public class TestEvent : IEvent
    {
        public string Message { get; set; }
        public int Id { get; set; }
    }

}
