using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Noodle.Attributes;

namespace Noodle.Modules
{
    [ModuleName("Information")]
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