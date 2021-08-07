using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageMagick;
using Microsoft.Extensions.Logging;
using Noodle.Hosting;
using Noodle.TypeReaders;

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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _commandService.AddTypeReader(typeof(EmoteType), new FileExtensionTypeReader());
            _commandService.AddTypeReader(typeof(TextAlignment), new TextAlignmentTypeReader());
            _commandService.AddTypeReader(typeof(FontStyleType), new FontStyleTypeTypeReader());
            _commandService.AddTypeReader(typeof(FontWeight), new FontWeightTypeReader());
            _commandService.AddTypeReader(typeof(FontStretch), new FontStretchTypeReader());
            _commandService.AddTypeReader(typeof(Emote), new EmoteTypeReader());
            
            await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
        }
    }
}