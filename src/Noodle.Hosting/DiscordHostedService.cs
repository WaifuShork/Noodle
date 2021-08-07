using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noodle.Hosting.Util;

namespace Noodle.Hosting
{
    internal class DiscordHostedService<T> : IHostedService where T: BaseSocketClient
    {
        private readonly ILogger<DiscordHostedService<T>> _logger;
        private readonly T _client;
        private readonly DiscordHostConfiguration _config;

        public DiscordHostedService(ILogger<DiscordHostedService<T>> logger, IOptions<DiscordHostConfiguration> options, LogAdapter<T> adapter, T client)
        {
            _logger = logger;
            _config = options.Value;
            _client = client;
            _client.Log += adapter.Log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord.NET hosted service is starting");
            
            try
            {
                await _client.LoginAsync(TokenType.Bot, _config.Token).WithCancellation(cancellationToken);
                await _client.StartAsync().WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Startup has been aborted, exiting...");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord.NET hosted service is stopping");
            try
            {
                await _client.StopAsync().WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogCritical("Discord.NET client could not be stopped within the given timeout and may have permanently deadlocked");
            }
        }
    }
}
