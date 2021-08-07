using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noodle.Hosting.Util;

namespace Noodle.Hosting
{
    internal class CommandServiceRegistrationHost : IHostedService
    {
        private readonly CommandService _commandService;
        private readonly ILogger<CommandServiceRegistrationHost> _logger;
        private readonly LogAdapter<CommandService> _adapter;

        public CommandServiceRegistrationHost(CommandService commandService, ILogger<CommandServiceRegistrationHost> logger, LogAdapter<CommandService> adapter)
        {
            _commandService = commandService;
            _logger = logger;
            _adapter = adapter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _commandService.Log += _adapter.Log;
            _logger.LogDebug("Registered logger for CommandService");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
