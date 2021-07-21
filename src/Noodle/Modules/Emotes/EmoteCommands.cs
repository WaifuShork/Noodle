using System.Linq;

using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule   
    {
        [Command("emoteupdate"), Alias("eu", "emotestatus", "emotecount", "es")]
        [RequireContext(ContextType.Guild)]
        public async Task DisplayEmoteCountAsync()
        {
            var guilds = Context.User.MutualGuilds;

            var counter = guilds.Where(guild => guild.Name.Contains("ShorkMotes")).Sum(guild => guild.Emotes.Count);

            var emotes = await Context.Guild.GetEmotesAsync();
            var animatedEmotes = emotes.Where(e => e.Animated).ToList();
            var normalEmotes = emotes.Where(e => !e.Animated).ToList();
            
            await SendAsync(CreateEmbed()
                    .WithTitle("Emote Status")
                    .AddField("Normal", normalEmotes.Count)
                    .AddField("Animated", animatedEmotes.Count)
                    .AddField("Guild Total", emotes.Count)
                    .AddField("True Total", counter)
                    .WithColor(Color.Blue));
        }
    }
}