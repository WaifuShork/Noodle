using System;
using System.Threading.Tasks;

using Serilog;

using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Addons.Interactive;

namespace Noodle
{
    public abstract class NoodleModuleBase : InteractiveBase<SocketCommandContext>
    {
        protected async Task<IMessage> NextMessageAsync(TimeSpan seconds)
        {
            return await NextMessageAsync(true, true, seconds);
        }

        protected async Task<IMessage> NextMessageAsync(double seconds)
        {
            return await NextMessageAsync(true, true, TimeSpan.FromSeconds(seconds));
        }

        protected async Task TimedDeletionAsync(string contents, bool isTTS, Embed embed, TimeSpan timeSpan)
        {
            try
            {
                var message = await Context.Channel.SendMessageAsync(contents, isTTS, embed);
                // Task.Delay provides a non-blocking timer for delays, avoid using Thread.Sleep in asynchronous operations 
                await Task.Delay(timeSpan);
                await message.DeleteAsync();
            }
            catch (HttpException e)
            {
                Log.Warning(e, "TimedDeletionAsync caught {Exception}\n\nMessage: {Message}", typeof(HttpException), e.Message);
            }
        }

        protected EmbedBuilder CreateEmbed(string title)
        {
            return new EmbedBuilder()
                .WithTitle(title)
                .WithCurrentTimestamp();
        }

        protected EmbedBuilder CreateEmbed()
        {
            return new EmbedBuilder().WithCurrentTimestamp();
        }
    }
}