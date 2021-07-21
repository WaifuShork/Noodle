using System;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;

namespace Noodle
{
    // You don't need to keep this class included, I just use it to extend functionality of InteractiveBase,
    // InteractiveBase is a command context built on top of ModuleBase for interactive commands, I've defined
    // a few helper methods below that you can call from any class that inherits from TemplateModuleBase
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

        // Useful for sending a message that you want to delete in X seconds
        protected async Task TimedDeletionAsync(string contents, bool isTTS, Embed embed, TimeSpan timeSpan)
        {
            try
            {
                var message = await Context.Channel.SendMessageAsync(contents, isTTS, embed);
                // Task.Delay provides a non-blocking timer for delays, avoid using Thread.Sleep in asynchronous operations 
                await Task.Delay(timeSpan);
                await message.DeleteAsync();
            }
            // Why try/catch a simple deletion of a message? I've had numerous occasions where my bot would throw when attempting to timed delete
            // I'm entirely unsure of the cause, but I started logging it just to keep up with when it does happen, you're more than welcome to remove this
            catch (HttpException e)
            {
                Log.Warning($"TimedDeletionAsync caught {typeof(HttpException)}\n\nMessage: {e.Message}\n\nInner Exception: {e.InnerException}");
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

        protected async Task<IMessage> SendSuccessEmbedAsync(string contents)
        {
            var embed = CreateEmbed("Success")
                .WithColor(Color.Green)
                .WithDescription(contents)
                .Build();

            return await Context.Channel.SendMessageAsync(string.Empty, false, embed);
        }

        protected async Task<IMessage> SendErrorEmbedAsync(string contents)
        {
            var embed = CreateEmbed("Error")
                .WithColor(Color.Red)
                .WithDescription(contents)
                .Build();
            
            return await Context.Channel.SendMessageAsync(string.Empty, false, embed);
        }

        protected async Task<IMessage> SendAsync(EmbedBuilder embed)
        {
            return await Context.Channel.SendMessageAsync(string.Empty, false, embed.Build());
        }
    }
}