using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Noodle.Attributes;
using Noodle.Extensions;

namespace Noodle.Modules
{
    public sealed partial class InformationModule
    {
        [Command("help")]
        [Summary("Prints out help for all the modules")]
        public async Task HelpAsync()
        {
            var prefix = _configuration["prefix"];
            
            var sb = new StringBuilder()
                .AppendLine($"**Usage:** {prefix}help <module>")
                .AppendLine();

            var availableModules = _commandService.GetAvailableModules();
            
            foreach (var module in availableModules)
            {
                var moduleName = module.GetAttribute<ModuleNameAttribute>();
                sb.AppendLine($"• {moduleName.Name}");
                sb.AppendLine($"⠀⠀- {module.Summary}");
                sb.AppendLine();
            }

            await Context.Channel.SendAsync(CreateEmbed("[Command Modules]")
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString()));
        }

        [Command("help")]
        [Summary("Prints out help for a specific command or module")]
        [Remarks("help <identifier>")]
        public async Task HelpSpecificAsync([Summary("The command or module to request help for"), Remainder] string identifier)
        {
            var commandInfo = _commandService.GetTargetCommand(identifier); 
            var moduleInfo = _commandService.GetTargetModule(identifier);

            var prefix = _configuration["prefix"];
            
            if (commandInfo == null && moduleInfo == null)
            {
                await SendErrorEmbedAsync($"Unable to locate any command or module by the name of '{identifier}'");
            }

            if (commandInfo != null && moduleInfo == null)
            {
                await commandInfo.DisplayCommandHelpAsync(Context, prefix);
            }

            if (moduleInfo != null)
            {
                await moduleInfo.DisplayModuleHelpAsync(Context, prefix);
            }
        }
    }
}