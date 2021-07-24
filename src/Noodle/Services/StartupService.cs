using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Noodle.TypeReaders;
using SQLitePCL;

namespace Noodle.Services
{
    public class StartupService : DiscordClientService
    {
        private readonly CommandService _commandService;
        private readonly IServiceProvider _provider;
        
        public StartupService(DiscordSocketClient client, 
                              ILogger<StartupService> logger, 
                              CommandService commandService,
                              IServiceProvider provider) : base(client, logger)
        {
            _commandService = commandService;
            _provider = provider;

        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return RunAsync(cancellationToken);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _commandService.AddTypeReader(typeof(EmoteType), new FileExtensionTypeReader());
            _commandService.AddTypeReader(typeof(Emote), new EmoteTypeReader());
            await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
        }
    }
}