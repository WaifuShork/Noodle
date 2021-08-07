using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Noodle.Hosting
{
    public abstract class DiscordServiceBase<T> : BackgroundService where T : BaseSocketClient
    {
        protected DiscordServiceBase(T client, ILogger logger)
        {
            Client = client;
            Logger = logger;
        }
        
        protected T Client { get; }
        protected ILogger Logger { get; }
        
        protected abstract override Task ExecuteAsync(CancellationToken stoppingToken);
    }
    
    public abstract class DiscordClientService : DiscordServiceBase<DiscordSocketClient>
    {
        /// <summary>
        /// Creates a new <see cref="DiscordClientService" />
        /// </summary>
        /// <param name="logger">The logger for this service</param>
        /// <param name="client">The discord client</param>
        protected DiscordClientService(DiscordSocketClient client, ILogger<DiscordClientService> logger) : base(client, logger)
        {
        }
    }
}