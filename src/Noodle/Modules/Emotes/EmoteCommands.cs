using System.Linq;

using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Noodle.Extensions;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule   
    {
        [Command("emoteupdate"), Alias("eu", "emotestatus", "emotecount", "es")]
        [Summary("Shows the current emote count in the emote servers")]
        [Remarks("es")]
        public async Task DisplayEmoteCountAsync()
        {
            var guilds = Context.User.MutualGuilds;

            var counter = guilds.Where(guild => guild.Name.Contains("ShorkMotes")).Sum(guild => guild.Emotes.Count);

            var emotes = await Context.Guild.GetEmotesAsync();
            var animatedEmotes = emotes.Where(e => e.Animated).ToList();
            var normalEmotes = emotes.Where(e => !e.Animated).ToList();
            
            await Context.Channel.SendAsync(CreateEmbed()
                    .WithTitle("Emote Status")
                    .AddField("Normal", normalEmotes.Count)
                    .AddField("Animated", animatedEmotes.Count)
                    .AddField("Guild Total", emotes.Count)
                    .AddField("True Total", counter)
                    .WithColor(Color.Blue));
        }
    }
}