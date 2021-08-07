using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Noodle.Services
{
    public class ShutdownService : DiscordClientService
    {
        private readonly IHostApplicationLifetime _application;
        
        public ShutdownService(DiscordSocketClient client, ILogger<ShutdownService> logger, IHostApplicationLifetime application) : base(client, logger)
        {
            _application = application;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Task.Run(() =>
            {
                _application.ApplicationStopping.Register(OnStopping);
                _application.ApplicationStopped.Register(OnStopped);
            }, cancellationToken);
        }

        private void OnStopping()
        {
            Log.Information("Stopping");
        }

        private void OnStopped()
        {
            var process = Process.GetCurrentProcess();
            process.Kill();
        }
    }
}