using System;
using System.Threading.Tasks;

using Serilog;

using Discord;
using Discord.Net;
using Discord.Commands;

namespace Noodle
{
    public abstract class NoodleModuleBase : ModuleBase<SocketCommandContext>
    {
        public IServiceProvider Provider { get; init; }

        protected async Task TimedDeletionAsync(string contents, bool isTTS, Embed embed, TimeSpan timeSpan)
        {
            try
            {
                var message = await Context.Channel.SendMessageAsync(contents, isTTS, embed);
                await Task.Delay(timeSpan);
                await message.DeleteAsync();
            }
            catch (HttpException e)
            {
                Log.Warning(e, "TimedDeletionAsync caught {Exception}\n\nMessage: {Message}", typeof(HttpException), e.Message);
            }
        }

        protected static EmbedBuilder CreateEmbed(string title)
        {
            return new EmbedBuilder()
                .WithTitle($"__{title}__")
                .WithCurrentTimestamp();
        }

        protected static EmbedBuilder CreateEmbed()
        {
            return new EmbedBuilder()
                .WithCurrentTimestamp();
        }
    }
}