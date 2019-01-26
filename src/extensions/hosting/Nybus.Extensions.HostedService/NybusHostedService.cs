using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nybus;

namespace Nybus
{
    public class NybusHostedService : IHostedService
    {
        private readonly ILogger<NybusHostedService> _logger;
        private readonly IBusHost _host;

        public NybusHostedService(IBusHost host, ILogger<NybusHostedService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");
            await _host.StartAsync();
            _logger.LogInformation("Started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping");
            await _host.StopAsync();
            _logger.LogInformation("Stopped");
        }
    }
}