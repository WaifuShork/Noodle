using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Noodle.Attributes;
using Serilog;
using Serilog.Events;

namespace Noodle.Extensions
{
    public static class CommandServiceExtensions
    {
        
        public static IEnumerable<ModuleInfo> GetAvailableModules(this CommandService commandService, ILogger logger = null, string contents = null, LogEventLevel level = LogEventLevel.Information)
        {
            if (logger != null && !string.IsNullOrWhiteSpace(contents))
            {
                logger.Write(level, contents);
            }
            
            return commandService.Modules.Where(m => m.HasAttribute<ModuleNameAttribute>());
        }

        public static CommandInfo GetTargetCommand(this CommandService commandService, string name, ILogger logger = null, string contents = null, LogEventLevel level = LogEventLevel.Information)
        {
            if (logger != null && !string.IsNullOrWhiteSpace(contents))
            {
                logger.Write(level, contents);
            }
            
            return commandService.Commands.FirstOrDefault(c =>
                c.Aliases.Any(a => 
                    string.Equals(name, a, StringComparison.InvariantCultureIgnoreCase)));
        }

        public static ModuleInfo GetTargetModule(this CommandService commandService, string name, ILogger logger = null, string contents = null, LogEventLevel level = LogEventLevel.Information)
        {
            if (logger != null && !string.IsNullOrWhiteSpace(contents))
            {
                logger.Write(level, contents);
            }
            
            return commandService.Modules.FirstOrDefault(m => 
                m.Aliases.Any(a => string.Equals(name, a, StringComparison.InvariantCultureIgnoreCase) ||
                                   string.Equals(m.Name.SanitizeModule(), name, StringComparison.InvariantCultureIgnoreCase))
            );
        }
    }
}