using Discord;
using Discord.Commands;
using Noodle.Extensions;
using Noodle.TypeReaders;
using System.Threading.Tasks;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule    
    {
        [Command("delmote")]
        [Summary("Deletes an emote from a server")]
        [Remarks("delmote <emote>")]
        public async Task DeleteEmoteAsync([Summary("The emote to delete"), OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote)
        {
            var em = await Context.Guild.GetEmoteAsync(emote.Id);
            try
            {
                await Context.Guild.DeleteEmoteAsync(em);
                await Context.Channel.SendSuccessAsync($"Deleted **{emote.Name}**");
            }
            catch
            {
                await Context.Channel.SendErrorAsync($"Unable to delete **{emote.Name}**");
            }
        }
    }
}