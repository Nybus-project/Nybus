using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Kralizek.Lambda;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nybus
{
    public abstract class NybusFunction<TInput> : Function
    {
        protected NybusFunction()
        {
            _busHost = ServiceProvider.GetRequiredService<IBusHost>();
        }

        private readonly IBusHost _busHost;

        public async Task FunctionHandlerAsync(TInput input, ILambdaContext context)
        {
            try
            {
                await _busHost.StartAsync().ConfigureAwait(false);

                using (var scope = ServiceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetService<Kralizek.Lambda.IEventHandler<TInput>>();

                    if (handler == null)
                    {
                        Logger.LogCritical($"No IEventHandler<{typeof(TInput).Name}> could be found.");
                        throw new InvalidOperationException($"No IEventHandler<{typeof(TInput).Name}> could be found.");
                    }

                    Logger.LogInformation("Invoking handler");
                    await handler.HandleAsync(input, context).ConfigureAwait(false);
                }
            }
            finally
            { 
                await _busHost.StopAsync().ConfigureAwait(false);
            }
        }

        protected void RegisterHandler<THandler>(IServiceCollection services) where THandler : class, Kralizek.Lambda.IEventHandler<TInput>
        {
            services.AddTransient<Kralizek.Lambda.IEventHandler<TInput>, THandler>();
        }
    }
}
