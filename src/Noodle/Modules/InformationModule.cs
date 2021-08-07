using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Noodle.Attributes;

namespace Noodle.Modules
{
    [ModuleName("Information")]
    [Summary("Provides information about the bot, users, server, or commands")]
    public sealed partial class InformationModule : NoodleModuleBase
    {
        private readonly IConfiguration _configuration;
        private readonly CommandService _commandService;
        
        public InformationModule(IConfiguration configuration, CommandService commandService)
        {
            _configuration = configuration;
            _commandService = commandService;
        }
    }
}