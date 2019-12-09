using System;
using System.Threading;
using System.Threading.Tasks;
using BigBubble.App;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BigBubble
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        BigBubbleClient _bigBubbleClient;

        public Worker(ILogger<Worker> logger, BigBubbleClient client)
        {
            _logger = logger;
            _bigBubbleClient = client;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _bigBubbleClient.InitChat(stoppingToken);

            this.Dispose();

            throw new TaskCanceledException();

        }
    }
}
