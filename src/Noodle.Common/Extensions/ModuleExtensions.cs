using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Noodle.Extensions
{
    public static class ModuleExtensions
    {
        public static bool HasAttribute<T>(this ModuleInfo info) where T : notnull, Attribute
        {
            return info.Attributes.Any(a => a.GetType() == typeof(T));
        }

        public static bool HasAttribute<T>(this CommandInfo info) where T : notnull, Attribute
        {
            return info.Attributes.Any(a => a.GetType() == typeof(T));
        }

        public static T GetAttribute<T>(this ModuleInfo info) where T : notnull, Attribute
        {
            return info.Attributes.FirstOrDefault(a => a.GetType() == typeof(T)) as T;
        }
        
        public static T GetAttribute<T>(this CommandInfo info) where T : notnull, Attribute
        {
            return info.Attributes.FirstOrDefault(a => a.GetType() == typeof(T)) as T;
        }

        private const string _noDescription = "No description provided.";
        private const string _noUsage = "No usage provided.";

        public static async Task DisplayCommandHelpAsync(this CommandInfo command, ICommandContext context, string prefix)
        {
            var title = command.Name;
            if (command.Aliases.Count > 1)
            {
                title += $"({string.Join('|', command.Aliases)})";
            }

            var description = new StringBuilder()
                .AppendLine(command.Summary.WithAlternative(_noDescription))
                .AppendLine()
                .AppendLine($"**Module**: {command.Module.Name.SanitizeModule()}")
                .AppendLine($"**Usage**: {(string.IsNullOrWhiteSpace(command.Remarks) ? _noUsage : prefix + command.Remarks)}");


            await context.Channel.SendAsync(new EmbedBuilder()
                .WithTitle(title)
                .WithColor(Color.Blue)
                .WithDescription(description.ToString())
                .WithFields(command.Parameters.Select(InfoToEmbedField)));
        }
        
        public static async Task DisplayModuleHelpAsync(this ModuleInfo module, ICommandContext context, string prefix)
        {
            var displayable = module.Commands.Where(c =>
            {
                return Task.Run(async () =>
                {
                    return (await c.CheckPreconditionsAsync(context)).IsSuccess;
                }).Result;
            }).ToList();

            var displayModuleHelp = displayable.Count > 0;
            if (displayModuleHelp)
            {
                var builder = new EmbedBuilder()
                    .WithTitle(module.Name.SanitizeModule())
                    .WithColor(Color.Blue)
                    .WithFields(displayable.Select(c => c.InfoToEmbedField(prefix)));

                if (!string.IsNullOrWhiteSpace(module.Summary))
                {
                    builder.WithDescription(module.Summary);
                }

                await context.Channel.SendAsync(builder);
            }
        }
        
        private static EmbedFieldBuilder InfoToEmbedField(ParameterInfo info)
        {
            return new EmbedFieldBuilder()
                .WithName(info.Name)
                .WithValue(info.Summary.WithAlternative(_noDescription))
                .WithIsInline(true);
        }

        private static EmbedFieldBuilder InfoToEmbedField(this CommandInfo info, string prefix)
        {
            return new EmbedFieldBuilder()
                .WithName(string.Concat(prefix, info.Remarks.WithAlternative(info.Name)))
                .WithValue(info.Summary.WithAlternative(_noUsage))
                .WithIsInline(true);
        }
    }
}